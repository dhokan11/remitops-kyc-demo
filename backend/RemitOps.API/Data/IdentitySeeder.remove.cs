using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RemitOps.API.Auth;
using RemitOps.API.Entities;
using RemitOps.API.Models;

namespace RemitOps.API.Data;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var seedOptions = scope.ServiceProvider.GetRequiredService<IOptions<SeedAdminOptions>>().Value;

        await db.Database.MigrateAsync();

        var roles = new[]
        {
            Roles.PlatformAdmin,
            Roles.OrgUnitAdmin,
            Roles.EndUser
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var existing = await userManager.FindByEmailAsync(seedOptions.Email);
        if (existing is null)
        {
            var admin = new ApplicationUser
            {
                UserName = seedOptions.UserName,
                Email = seedOptions.Email,
                EmailConfirmed = true,
                FirstName = seedOptions.FirstName,
                LastName = seedOptions.LastName,
                UserType = Roles.PlatformAdmin,
                RegistrationStatus = "Active",
                IsActive = true
            };

            var created = await userManager.CreateAsync(admin, seedOptions.Password);
            if (!created.Succeeded)
            {
                var errors = string.Join("; ", created.Errors.Select(e => e.Description));
                throw new Exception($"Seed admin creation failed: {errors}");
            }

            await userManager.AddToRoleAsync(admin, Roles.PlatformAdmin);
        }
    }
}