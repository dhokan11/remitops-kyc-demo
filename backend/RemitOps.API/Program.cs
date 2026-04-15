// ~/projects/remitops/backend/RemitOps.API/Program.cs
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RemitOps.API.Auth;
using RemitOps.API.Data;
using RemitOps.API.Entities;
using RemitOps.API.Models;
using RemitOps.API.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// Serilog
// ========================================
// Replace your existing Serilog block with this:
builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .MinimumLevel.Debug() // This is the "Noise" switch
        .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information) // Keep framework logs clean
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", Serilog.Events.LogEventLevel.Information) // Set to Information to see SQL
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console(); // Ensures output goes to your terminal
});


Log.Information("========================================");
Log.Information("🚀 RemitOps API Starting Up");
Log.Information("========================================");
Log.Information("Environment: {Environment}", builder.Environment.EnvironmentName);
Log.Information("Machine: {Machine}", Environment.MachineName);
Log.Information("Date/Time: {Now}", DateTime.Now);
Log.Information("========================================");

// ========================================
// Configuration binding
// ========================================
builder.Services.Configure<DatabaseSeedingOptions>(
    builder.Configuration.GetSection("DatabaseSeeding"));
builder.Services.Configure<AuditSeedingOptions>(
    builder.Configuration.GetSection("AuditSeeding"));
builder.Services.Configure<SeedAdminOptions>(
    builder.Configuration.GetSection("SeedAdmin"));
builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection("Jwt"));

// ========================================
// DbContext and connection factory
// ========================================
Log.Information("Setting up database context...");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var cs = builder.Configuration.GetConnectionString("DefaultConnection")
             ?? throw new InvalidOperationException("DefaultConnection missing.");
    Log.Information("Connection String: {ConnectionString}", cs);
    options.UseSqlServer(cs);
});

builder.Services.AddScoped<IDbConnectionFactory, SqlConnectionFactory>();

// ========================================
// Identity
// ========================================
Log.Information("Configuring Identity with password policies...");
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 8;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// ========================================
// JWT auth
// ========================================
Log.Information("Configuring JWT authentication...");
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtOptions = jwtSection.Get<JwtOptions>() ?? new JwtOptions();

Log.Information("JWT Settings - Issuer: {Issuer}, Audience: {Audience}", jwtOptions.Issuer, jwtOptions.Audience);

var keyBytes = Encoding.UTF8.GetBytes(
    jwtOptions.Key ?? throw new InvalidOperationException("Jwt:Key is missing"));

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

builder.Services.AddScoped<JwtTokenService>();

// ========================================
// Authorization policies
// ========================================
Log.Information("Configuring authorization policies...");
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PlatformAdminOnly",
        p => p.RequireRole(Roles.PlatformAdmin));

    options.AddPolicy(Policies.ManagePlatform,
        p => p.RequireRole(Roles.PlatformAdmin));

    options.AddPolicy(Policies.ManageTenants,
        p => p.RequireRole(Roles.PlatformAdmin));

    options.AddPolicy(Policies.ManageOrgUnits,
        p => p.RequireRole(Roles.PlatformAdmin));

    options.AddPolicy(Policies.ViewOwnProfile,
        p => p.RequireAuthenticatedUser());
});

// ========================================
// Domain services
// ========================================
Log.Information("Registering core services...");

// Admin service used by TenantsController, OrgUnitsController, UsersController, etc.
builder.Services.AddScoped<RemitOps.API.Services.Admin.IAdminService, RemitOps.API.Services.Admin.AdminService>();

// Audit logging service
builder.Services.AddScoped<IAuditService, AuditService>();

// ========================================
// Database initialization + seeding services
// ========================================
Log.Information("Configuring database initialization services...");
builder.Services.AddScoped<DatabaseBootstrapper>();
builder.Services.AddScoped<IdentityMigrationRunner>();
builder.Services.AddScoped<IdentitySeedService>();
builder.Services.AddScoped<SqlSchemaSeeder>();
builder.Services.AddScoped<DemoDataSeeder>();
builder.Services.AddScoped<AppStartupSeeder>();

// ========================================
// MVC + Swagger
// ========================================
Log.Information("Configuring MVC and Swagger...");
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ========================================
// CORS, HTTP logging, etc.
// ========================================
Log.Information("Configuring HTTP and form options...");
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.All;
});

Log.Information("Configuring CORS policy for frontend...");
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendCors", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173", "http://localhost:5174")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

Log.Information("========================================");
Log.Information("✅ All services configured successfully");
Log.Information("========================================");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Shows detailed error screens
}

// ========================================
// Centralized, idempotent database seeding
// ========================================
Log.Information("========================================");
Log.Information("Running AppStartupSeeder (database + identity + demo data)...");
Log.Information("========================================");

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<AppStartupSeeder>();
    await seeder.RunAsync();
}

Log.Information("========================================");
Log.Information("Database initialization complete");
Log.Information("========================================");

// ========================================
// Middleware pipeline
// ========================================
Log.Information("Setting up middleware pipeline...");

if (app.Environment.IsDevelopment())
{
    Log.Information("Development environment detected - enabling Swagger");
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("FrontendCors");
app.UseHttpLogging();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

Log.Information("Application startup complete");
app.Run();