using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RemitOps.API.Entities;

namespace RemitOps.API.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // ===== Identity & Multi-Tenancy =====
    public DbSet<Tenant> Tenants => Set<Tenant>();

    // ===== Organization Structure =====
    public DbSet<OrgUnit> OrgUnits => Set<OrgUnit>();
    public DbSet<GeoLocation> GeoLocations => Set<GeoLocation>();

    // ===== Remittance Management =====
    public DbSet<RemittanceRequest> RemittanceRequests => Set<RemittanceRequest>();
    public DbSet<RemittanceAudit> RemittanceAudits => Set<RemittanceAudit>();

    // ===== Compliance =====
    public DbSet<KycDocument> KycDocuments => Set<KycDocument>();

    // ===== Tagging System =====
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<OrgUnitTag> OrgUnitTags => Set<OrgUnitTag>();
    public DbSet<RemittanceRequestTag> RemittanceRequestTags => Set<RemittanceRequestTag>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // ===== EXPLICIT REMITTANCE-ORGUNIT RELATIONSHIPS (MUST BE FIRST) =====
        builder.Entity<RemittanceRequest>()
            .HasOne(r => r.SourceOrgUnit)
            .WithMany(o => o.SourceRemittances)
            .HasForeignKey(r => r.SourceOrgUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<RemittanceRequest>()
            .HasOne(r => r.DestinationOrgUnit)
            .WithMany(o => o.DestinationRemittances)
            .HasForeignKey(r => r.DestinationOrgUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        base.OnModelCreating(builder);

        // ===== TENANT CONFIGURATION =====
        builder.Entity<Tenant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Latitude).HasPrecision(18, 10);
            entity.Property(e => e.Longitude).HasPrecision(18, 10);
        });

        // ===== GEOLOCATION CONFIGURATION =====
        builder.Entity<GeoLocation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CountryCode).IsRequired().HasMaxLength(2);
            entity.Property(e => e.CountryName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.City).IsRequired().HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(100);
            entity.Property(e => e.PostalCode).HasMaxLength(20);
            entity.Property(e => e.Latitude).HasPrecision(9, 6);
            entity.Property(e => e.Longitude).HasPrecision(9, 6);
            entity.Property(e => e.TimeZone).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasIndex(e => e.CountryCode);
            entity.HasIndex(e => e.City);
        });

        // ===== ORGUNIT CONFIGURATION =====
        builder.Entity<OrgUnit>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.ContactEmail).HasMaxLength(255);
            entity.Property(e => e.Latitude).HasPrecision(9, 6);
            entity.Property(e => e.Longitude).HasPrecision(9, 6);
            entity.Property(e => e.CountryCode).HasMaxLength(2);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(100);
            entity.Property(e => e.PostalCode).HasMaxLength(20);
            entity.Property(e => e.TimeZone).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.OrgUnits)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.GeoLocation)
                .WithMany(g => g.OrgUnits)
                .HasForeignKey(e => e.GeoLocationId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(e => e.Tags)
                .WithOne(t => t.OrgUnit)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.CountryCode);
            entity.HasIndex(e => e.City);
        });

        // ===== APPLICATION USER CONFIGURATION =====
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.UserType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.RegistrationStatus).IsRequired().HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.Users)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.OrgUnit)
                .WithMany(ou => ou.Users)
                .HasForeignKey(e => e.OrgUnitId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.OrgUnitId);
        });

        // ===== REMITTANCE REQUEST CONFIGURATION =====
        builder.Entity<RemittanceRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BeneficiaryName).HasMaxLength(200);
            entity.Property(e => e.BeneficiaryCountryCode).HasMaxLength(2);
            entity.Property(e => e.BeneficiaryCity).HasMaxLength(100);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Currency).IsRequired().HasMaxLength(3);
            entity.Property(e => e.CurrentQueue).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CurrentStatus).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Priority).HasMaxLength(20);

            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.RemittanceRequests)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.SubmittedByUser)
                .WithMany()
                .HasForeignKey(e => e.SubmittedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.LastActionByUser)
                .WithMany()
                .HasForeignKey(e => e.LastActionByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasMany(e => e.Tags)
                .WithOne(t => t.RemittanceRequest)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.AuditTrail)
                .WithOne(a => a.RemittanceRequest)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.SourceOrgUnitId);
            entity.HasIndex(e => e.DestinationOrgUnitId);
            entity.HasIndex(e => e.CurrentStatus);
            entity.HasIndex(e => e.CurrentQueue);
            entity.HasIndex(e => e.CreatedAtUtc);
            entity.HasIndex(e => e.BeneficiaryCountryCode);
        });

        // ===== REMITTANCE AUDIT CONFIGURATION =====
        builder.Entity<RemittanceAudit>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Notes).HasMaxLength(500);

            entity.HasOne(e => e.RemittanceRequest)
                .WithMany(r => r.AuditTrail)
                .HasForeignKey(e => e.RemittanceRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.PerformedByUser)
                .WithMany()
                .HasForeignKey(e => e.PerformedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasIndex(e => e.RemittanceRequestId);
            entity.HasIndex(e => e.PerformedAtUtc);
        });

        // ===== KYC DOCUMENT CONFIGURATION =====
        builder.Entity<KycDocument>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.IdentityUserId)
                .IsRequired();

            entity.Property(e => e.DocumentType)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.FileName)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.ReviewStatus)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasOne(e => e.Tenant)
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.OrgUnit)
                .WithMany(ou => ou.KycDocuments)
                .HasForeignKey(e => e.OrgUnitId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.IdentityUserId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasIndex(e => e.IdentityUserId);
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.OrgUnitId);
            entity.HasIndex(e => e.ReviewStatus);
        });

        // ===== TAG CONFIGURATION =====
        builder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Color).HasMaxLength(7);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.Category);
        });

        // ===== ORGUNIT TAG (JUNCTION) CONFIGURATION =====
        builder.Entity<OrgUnitTag>(entity =>
        {
            entity.HasKey(e => new { e.OrgUnitId, e.TagId });

            entity.HasOne(e => e.OrgUnit)
                .WithMany(ou => ou.Tags)
                .HasForeignKey(e => e.OrgUnitId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Tag)
                .WithMany(t => t.OrgUnitTags)
                .HasForeignKey(e => e.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.OrgUnitId);
            entity.HasIndex(e => e.TagId);
        });

        // ===== REMITTANCE REQUEST TAG (JUNCTION) CONFIGURATION =====
        builder.Entity<RemittanceRequestTag>(entity =>
        {
            entity.HasKey(e => new { e.RemittanceRequestId, e.TagId });

            entity.HasOne(e => e.RemittanceRequest)
                .WithMany(r => r.Tags)
                .HasForeignKey(e => e.RemittanceRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Tag)
                .WithMany(t => t.RemittanceRequestTags)
                .HasForeignKey(e => e.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.RemittanceRequestId);
            entity.HasIndex(e => e.TagId);
        });
    }
}