using System;
using System.Collections.Generic;
using System.Data;              // <- add this line
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using RemitOps.API.Auth;
using RemitOps.API.Data;
using RemitOps.API.Entities;

namespace RemitOps.API.Services
{
    public class DemoDataSeeder
    {
        private readonly IDbConnectionFactory _factory;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<DemoDataSeeder> _logger;

        public DemoDataSeeder(
            IDbConnectionFactory factory,
            UserManager<ApplicationUser> userManager,
            ILogger<DemoDataSeeder> logger)
        {
            _factory = factory;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            using var db = _factory.CreateConnection();

            var before = await GetCountsAsync(db);
            _logger.LogInformation("DemoDataSeeder: starting. Before = {@Counts}", before);

            await SeedReferenceDataAsync();
            await SeedTenantsAndOrgUnitsAsync();
            await SeedUsersAsync();
            await SeedKycAndTransactionsAsync();
            await SeedAuditAsync();

            var after = await GetCountsAsync(db);
            _logger.LogInformation("DemoDataSeeder: completed. After = {@Counts}", after);
        }

        private static async Task<object> GetCountsAsync(System.Data.IDbConnection db)
        {
            var sql = @"
SELECT
    TenantCount  = (SELECT COUNT(*) FROM dbo.Tenants),
    OrgUnitCount = CASE WHEN OBJECT_ID('dbo.OrgUnits','U') IS NULL THEN 0 ELSE (SELECT COUNT(*) FROM dbo.OrgUnits) END,
    NavItemCount = CASE WHEN OBJECT_ID('dbo.NavigationItems','U') IS NULL THEN 0 ELSE (SELECT COUNT(*) FROM dbo.NavigationItems) END,
    TagCount     = CASE WHEN OBJECT_ID('dbo.Tags','U') IS NULL THEN 0 ELSE (SELECT COUNT(*) FROM dbo.Tags) END,
    TxCount      = CASE WHEN OBJECT_ID('dbo.RemittanceTransactions','U') IS NULL THEN 0 ELSE (SELECT COUNT(*) FROM dbo.RemittanceTransactions) END,
    AuditCount   = CASE WHEN OBJECT_ID('dbo.AuditLog','U') IS NULL THEN 0 ELSE (SELECT COUNT(*) FROM dbo.AuditLog) END;
";
            return await db.QuerySingleAsync<object>(sql);
        }

        private async Task SeedReferenceDataAsync()
        {
            using var db = _factory.CreateConnection();

            await db.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM dbo.Tags WHERE TagKey = 'corridor-east-africa')
BEGIN
    INSERT INTO dbo.Tags (Id, TagKey, CreatedAtUtc) VALUES
    (NEWID(), 'corridor-east-africa', GETUTCDATE()),
    (NEWID(), 'high-volume',           GETUTCDATE()),
    (NEWID(), 'kyc-sensitive',         GETUTCDATE()),
    (NEWID(), 'priority-branch',       GETUTCDATE()),
    (NEWID(), 'urban-hub',             GETUTCDATE());
END;

IF NOT EXISTS (SELECT 1 FROM dbo.NavigationItems WHERE RoleName = 'PlatformAdmin')
BEGIN
    INSERT INTO dbo.NavigationItems(RoleName, Label, Route, SortOrder) VALUES
    ('PlatformAdmin','Overview','/admin',1),
    ('PlatformAdmin','Tenants','/admin/tenants',2),
    ('PlatformAdmin','Org Units','/admin/org-units',3),
    ('PlatformAdmin','Users','/admin/users',4),
    ('PlatformAdmin','KYC Review','/admin/kyc-review',5),
    ('PlatformAdmin','Transactions','/admin/transactions',6),
    ('PlatformAdmin','Reports','/admin/reports',7),
    ('PlatformAdmin','Settings','/admin/settings',8),
    ('PlatformAdmin','Audit Trail','/admin/audit-trail',9),

    ('OrgUnitAdmin','Overview','/org-unit',1),
    ('OrgUnitAdmin','Queue','/org-unit/queue',2),
    ('OrgUnitAdmin','Users','/org-unit/users',3),
    ('OrgUnitAdmin','Reports','/org-unit/reports',4),

    ('EndUser','Overview','/app',1),
    ('EndUser','Transfers','/app/transfers',2),
    ('EndUser','KYC','/app/kyc',3),
    ('EndUser','Profile','/app/profile',4);
END;
");
        }

        private async Task SeedTenantsAndOrgUnitsAsync()
        {
            using var db = _factory.CreateConnection();

            // 1) Base tenants (no geo assumptions)
            await db.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM dbo.Tenants WHERE Code = 'TEN-001')
BEGIN
    INSERT INTO dbo.Tenants (Id, Code, Name, IsActive, CreatedAtUtc)
    VALUES
    (NEWID(), 'TEN-001', 'Dahab Hargeisa',  1, SYSUTCDATETIME()),
    (NEWID(), 'TEN-002', 'Dahab Mogadishu', 1, SYSUTCDATETIME()),
    (NEWID(), 'TEN-003', 'Dahab Nairobi',   1, SYSUTCDATETIME()),
    (NEWID(), 'TEN-004', 'Dahab Dubai',     1, SYSUTCDATETIME()),
    (NEWID(), 'TEN-005', 'Dahab London',    1, SYSUTCDATETIME());
END;
");

            // 2) Best-effort geo patch: only if columns exist, and never fail seeding
            try
            {
                await db.ExecuteAsync(@"
IF COL_LENGTH('dbo.Tenants', 'CountryCode') IS NOT NULL
   AND COL_LENGTH('dbo.Tenants', 'City') IS NOT NULL
   AND COL_LENGTH('dbo.Tenants', 'Latitude') IS NOT NULL
   AND COL_LENGTH('dbo.Tenants', 'Longitude') IS NOT NULL
BEGIN
    UPDATE t
    SET CountryCode = v.CountryCode,
        City        = v.City,
        Latitude    = v.Latitude,
        Longitude   = v.Longitude
    FROM dbo.Tenants t
    JOIN (VALUES
        ('TEN-001','SO','Hargeisa',  9.5600, 44.0650),
        ('TEN-002','SO','Mogadishu', 2.0469, 45.3182),
        ('TEN-003','KE','Nairobi',  -1.2864, 36.8172),
        ('TEN-004','AE','Dubai',    25.2048, 55.2708),
        ('TEN-005','GB','London',   51.5072,-0.1276)
    ) AS v(Code, CountryCode, City, Latitude, Longitude)
      ON t.Code = v.Code;
END;
");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "DemoDataSeeder: geo patch for Tenants failed; continuing without geo.");
            }



            //_logger.LogInformation("DemoDataSeeder: patching tenant geo via spPatchTenantGeo...");

            //await db.ExecuteAsync("EXEC dbo.spPatchTenantGeo;");

            // 3) OrgUnits with GUID TenantId
            await db.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM dbo.OrgUnits WHERE Code = 'OU-001')
BEGIN
    DECLARE @t1 UNIQUEIDENTIFIER = (SELECT Id FROM dbo.Tenants WHERE Code = 'TEN-001');
    DECLARE @t2 UNIQUEIDENTIFIER = (SELECT Id FROM dbo.Tenants WHERE Code = 'TEN-002');
    DECLARE @t3 UNIQUEIDENTIFIER = (SELECT Id FROM dbo.Tenants WHERE Code = 'TEN-003');
    DECLARE @t4 UNIQUEIDENTIFIER = (SELECT Id FROM dbo.Tenants WHERE Code = 'TEN-004');
    DECLARE @t5 UNIQUEIDENTIFIER = (SELECT Id FROM dbo.Tenants WHERE Code = 'TEN-005');

    INSERT INTO dbo.OrgUnits (Id, TenantId, Name, Code, CountryCode, City, Latitude, Longitude, Status, CreatedAtUtc, UpdatedAtUtc)
    VALUES
    (NEWID(), @t1, 'Hargeisa North',    'OU-001', 'SO', 'Hargeisa',  9.5700, 44.0600, 'Active', SYSUTCDATETIME(), SYSUTCDATETIME()),
    (NEWID(), @t1, 'Hargeisa South',    'OU-002', 'SO', 'Hargeisa',  9.5400, 44.0800, 'Active', SYSUTCDATETIME(), SYSUTCDATETIME()),
    (NEWID(), @t1, 'Berbera Branch',    'OU-003', 'SO', 'Berbera',  10.4396, 45.0164, 'Active', SYSUTCDATETIME(), SYSUTCDATETIME()),
    (NEWID(), @t2, 'Mogadishu Central', 'OU-004', 'SO', 'Mogadishu', 2.0380, 45.3436, 'Active', SYSUTCDATETIME(), SYSUTCDATETIME()),
    (NEWID(), @t2, 'Kismayo Branch',    'OU-005', 'SO', 'Kismayo',  -0.3582, 42.5454, 'Active', SYSUTCDATETIME(), SYSUTCDATETIME()),
    (NEWID(), @t2, 'Baidoa Branch',     'OU-006', 'SO', 'Baidoa',   3.1138, 43.6498, 'Active', SYSUTCDATETIME(), SYSUTCDATETIME()),
    (NEWID(), @t3, 'Nairobi Eastleigh', 'OU-007', 'KE', 'Nairobi', -1.2720, 36.8500, 'Active', SYSUTCDATETIME(), SYSUTCDATETIME()),
    (NEWID(), @t3, 'Nairobi CBD',       'OU-008', 'KE', 'Nairobi', -1.2833, 36.8167, 'Active', SYSUTCDATETIME(), SYSUTCDATETIME()),
    (NEWID(), @t3, 'Mombasa Branch',    'OU-009', 'KE', 'Mombasa', -4.0435, 39.6682, 'Active', SYSUTCDATETIME(), SYSUTCDATETIME()),
    (NEWID(), @t4, 'Dubai Deira',       'OU-010','AE', 'Dubai',    25.2750, 55.3070, 'Active', SYSUTCDATETIME(), SYSUTCDATETIME()),
    (NEWID(), @t4, 'Dubai Karama',      'OU-011','AE', 'Dubai',    25.2510, 55.3000, 'Active', SYSUTCDATETIME(), SYSUTCDATETIME()),
    (NEWID(), @t4, 'Sharjah Branch',    'OU-012','AE', 'Sharjah',  25.3463, 55.4209, 'Active', SYSUTCDATETIME(), SYSUTCDATETIME()),
    (NEWID(), @t5, 'London Wembley',    'OU-013','GB', 'London',   51.5520, -0.2960, 'Active', SYSUTCDATETIME(), SYSUTCDATETIME()),
    (NEWID(), @t5, 'London Whitechapel','OU-014','GB','London',    51.5190, -0.0590, 'Active', SYSUTCDATETIME(), SYSUTCDATETIME()),
    (NEWID(), @t5, 'Birmingham Branch', 'OU-015','GB','Birmingham',52.4862,-1.8904, 'Active', SYSUTCDATETIME(), SYSUTCDATETIME());
END;
");
        }

        private async Task SeedUsersAsync()
        {
            var orgAdmins = Enumerable.Range(1, 15)
                .Select(i => $"orgadmin{i:00}@remitops.local")
                .ToList();

            var endUsers = Enumerable.Range(1, 50)
                .Select(i => $"enduser{i:00}@remitops.local")
                .ToList();

            foreach (var email in orgAdmins)
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        FirstName = "Org",
                        LastName = "Admin",
                        EmailConfirmed = true
                    };
                    await _userManager.CreateAsync(user, "RemitOps@12345");
                }

                if (!await _userManager.IsInRoleAsync(user, Roles.OrgUnitAdmin))
                    await _userManager.AddToRoleAsync(user, Roles.OrgUnitAdmin);
            }

            foreach (var email in endUsers)
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        FirstName = "End",
                        LastName = "User",
                        EmailConfirmed = true
                    };
                    await _userManager.CreateAsync(user, "RemitOps@12345");
                }

                if (!await _userManager.IsInRoleAsync(user, Roles.EndUser))
                    await _userManager.AddToRoleAsync(user, Roles.EndUser);
            }
        }

        private async Task SeedKycAndTransactionsAsync()
        {
            using var db = _factory.CreateConnection();

            if (ObjectMissing(db, "dbo.OrgUnits") || ObjectMissing(db, "dbo.RemittanceTransactions"))
                return;

            var users = (await db.QueryAsync<(string Id, string Email)>(
                "SELECT Id, Email FROM AspNetUsers WHERE Email LIKE 'enduser%@remitops.local'")).ToList();


            var orgUnits = (await db.QueryAsync<(Guid Id, Guid TenantId)>(
                "SELECT Id, TenantId FROM dbo.OrgUnits")).ToList();

            if (!users.Any() || !orgUnits.Any())
                return;

            foreach (var user in users)
            {
                var idx = Math.Abs(user.Email.GetHashCode());
                var ou = orgUnits[idx % orgUnits.Count];
                var dest = orgUnits[(idx + 3) % orgUnits.Count];

                await db.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM dbo.UserProfiles WHERE IdentityUserId = @IdentityUserId)
BEGIN
    INSERT INTO dbo.UserProfiles
    (IdentityUserId, TenantId, OrgUnitId, DisplayName, RoleName, CountryCode, City, Latitude, Longitude, Status)
    VALUES
    (@IdentityUserId, @TenantId, @OrgUnitId, @DisplayName, 'EndUser', @CountryCode, @City, @Latitude, @Longitude, 'Active');
END;
", new
                {
                    IdentityUserId = user.Id,
                    TenantId = ou.TenantId,
                    OrgUnitId = ou.Id,
                    DisplayName = user.Email.Replace("@remitops.local", ""),
                    CountryCode = "SO",
                    City = "Hargeisa",
                    Latitude = 9.56m,
                    Longitude = 44.06m
                });

                await db.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM dbo.KycDocuments WHERE IdentityUserId = @IdentityUserId)
BEGIN
    INSERT INTO dbo.KycDocuments
    (IdentityUserId, TenantId, OrgUnitId, DocumentType, FileName, ReviewStatus)
    VALUES
    (@IdentityUserId, @TenantId, @OrgUnitId, 'Passport',       CONCAT(@DocPrefix, '-passport.pdf'), 'Approved'),
    (@IdentityUserId, @TenantId, @OrgUnitId, 'ProofOfAddress', CONCAT(@DocPrefix, '-address.pdf'),  'Pending');
END;
", new
                {
                    IdentityUserId = user.Id,
                    TenantId = ou.TenantId,
                    OrgUnitId = ou.Id,
                    DocPrefix = user.Email.Split('@')[0]
                });

                for (var i = 1; i <= 3; i++)
                {
                    var txRef = $"TXN-{user.Email.Split('@')[0]}-{i:00}";
                    await db.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM dbo.RemittanceTransactions WHERE TransactionRef = @TransactionRef)
BEGIN
    INSERT INTO dbo.RemittanceTransactions
    (TransactionRef, IdentityUserId, TenantId, SourceOrgUnitId, DestinationOrgUnitId, Amount, CurrencyCode, Status, KycStatus)
    VALUES
    (@TransactionRef, @IdentityUserId, @TenantId, @SourceOrgUnitId, @DestinationOrgUnitId, @Amount, 'USD', @Status, @KycStatus);
END;
", new
                    {
                        TransactionRef = txRef,
                        IdentityUserId = user.Id,
                        TenantId = ou.TenantId,
                        SourceOrgUnitId = ou.Id,
                        DestinationOrgUnitId = dest.Id,
                        Amount = 100 + (i * 25),
                        Status = i == 1 ? "Completed" : i == 2 ? "Pending" : "Flagged",
                        KycStatus = i == 1 ? "Approved" : "Pending"
                    });
                }
            }
        }

        private static bool ObjectMissing(IDbConnection db, string name)
        {
            return db.ExecuteScalar<int>(
                "SELECT CASE WHEN OBJECT_ID(@name, 'U') IS NULL THEN 1 ELSE 0 END",
                new { name }) == 1;
        }

        private async Task SeedAuditAsync()
        {
            using var db = _factory.CreateConnection();

            if (ObjectMissing(db, "dbo.AuditLog"))
                return;

            await db.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM dbo.AuditLog WHERE ActorEmail = 'platform.admin@remitops.local')
BEGIN
    INSERT INTO dbo.AuditLog
    (ActorUserId, ActorEmail, ActionType, EntityType, EntityId, PayloadJson, IpAddress, UserAgent, CreatedAtUtc)
    VALUES
    ('seed-platform-admin','platform.admin@remitops.local','LOGIN','AUTH','seed-platform-admin','{""role"":""PlatformAdmin""}','127.0.0.1','AutoSeed',DATEADD(MINUTE,-60,SYSUTCDATETIME())),
    ('seed-platform-admin','platform.admin@remitops.local','CREATE','TENANT','1','{""count"":5}','127.0.0.1','AutoSeed',DATEADD(MINUTE,-55,SYSUTCDATETIME())),
    ('seed-platform-admin','platform.admin@remitops.local','CREATE','ORG_UNIT','1','{""count"":15}','127.0.0.1','AutoSeed',DATEADD(MINUTE,-50,SYSUTCDATETIME())),
    ('seed-platform-admin','platform.admin@remitops.local','CREATE','USER','1','{""orgUnitAdmins"":15,""endUsers"":50}','127.0.0.1','AutoSeed',DATEADD(MINUTE,-45,SYSUTCDATETIME())),
    ('seed-platform-admin','platform.admin@remitops.local','VIEW_AUDIT','AUDIT','0',NULL,'127.0.0.1','AutoSeed',DATEADD(MINUTE,-40,SYSUTCDATETIME()));
END;
");
        }
    }
}