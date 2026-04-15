using System.Data;
using Dapper;
using RemitOps.API.Data;
using Microsoft.Extensions.Logging;

namespace RemitOps.API.Services
{
    public class SqlSchemaSeeder
    {
        private readonly IDbConnectionFactory _factory;
        private readonly ILogger<SqlSchemaSeeder> _logger;

        public SqlSchemaSeeder(IDbConnectionFactory factory, ILogger<SqlSchemaSeeder> logger)
        {
            _factory = factory;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            using var db = _factory.CreateConnection();

            _logger.LogInformation("SqlSchemaSeeder: ensuring tables, FKs, indexes, and procs...");
            await EnsureTablesAndColumnsAsync(db);
            await EnsureIndexesAsync(db);
            await EnsureStoredProcsAsync(db);
            _logger.LogInformation("SqlSchemaSeeder: completed.");
        }

        private static async Task EnsureTablesAndColumnsAsync(IDbConnection db)
        {
            var sql = @"
-- =======================
-- Tenants (EF-created): patch geo columns if missing
-- =======================
IF OBJECT_ID('dbo.Tenants', 'U') IS NOT NULL
BEGIN
    BEGIN
    IF COL_LENGTH('dbo.Tenants', 'CountryCode') IS NULL
        ALTER TABLE dbo.Tenants ADD CountryCode nvarchar(10) NULL;

    IF COL_LENGTH('dbo.Tenants', 'City') IS NULL
        ALTER TABLE dbo.Tenants ADD City nvarchar(100) NULL;

    IF COL_LENGTH('dbo.Tenants', 'Latitude') IS NULL
        ALTER TABLE dbo.Tenants ADD Latitude decimal(9,6) NULL;

    IF COL_LENGTH('dbo.Tenants', 'Longitude') IS NULL
        ALTER TABLE dbo.Tenants ADD Longitude decimal(9,6) NULL;

    IF COL_LENGTH('dbo.Tenants', 'Status') IS NULL
        ALTER TABLE dbo.Tenants ADD Status nvarchar(30) NOT NULL CONSTRAINT DF_Tenants_Status DEFAULT('Active');
END;
END;

-- =======================
-- OrgUnits (GUID TenantId -> Tenants.Id)
-- =======================
IF OBJECT_ID('dbo.OrgUnits', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrgUnits
    (
        OrgUnitId    INT IDENTITY(1,1) PRIMARY KEY,
        TenantId     UNIQUEIDENTIFIER NOT NULL,
        Name         NVARCHAR(200) NOT NULL,
        Code         NVARCHAR(50)  NOT NULL UNIQUE,
        CountryCode  NVARCHAR(10)  NULL,
        City         NVARCHAR(100) NULL,
        Latitude     DECIMAL(9,6)  NULL,
        Longitude    DECIMAL(9,6)  NULL,
        Status       NVARCHAR(30)  NOT NULL DEFAULT 'Active',
        CreatedAtUtc DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME()
    );
END;

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = 'FK_OrgUnits_Tenants_TenantId'
      AND parent_object_id = OBJECT_ID('dbo.OrgUnits')
)
BEGIN
    ALTER TABLE dbo.OrgUnits
        ADD CONSTRAINT FK_OrgUnits_Tenants_TenantId
        FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id);
END;

-- =======================
-- UserProfiles
-- =======================
IF OBJECT_ID('dbo.UserProfiles', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserProfiles
    (
        UserProfileId  INT IDENTITY(1,1) PRIMARY KEY,
        IdentityUserId NVARCHAR(450) NOT NULL,
        TenantId       UNIQUEIDENTIFIER NULL,
        OrgUnitId      INT NULL,
        DisplayName    NVARCHAR(200) NOT NULL,
        RoleName       NVARCHAR(50)  NOT NULL,
        CountryCode    NVARCHAR(10)  NULL,
        City           NVARCHAR(100) NULL,
        Latitude       DECIMAL(9,6)  NULL,
        Longitude      DECIMAL(9,6)  NULL,
        Status         NVARCHAR(30)  NOT NULL DEFAULT 'Active',
        CreatedAtUtc   DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME()
    );
END;

-- =======================
-- NavigationItems
-- =======================
IF OBJECT_ID('dbo.NavigationItems', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.NavigationItems
    (
        NavigationItemId INT IDENTITY(1,1) PRIMARY KEY,
        RoleName         NVARCHAR(50)  NOT NULL,
        Label            NVARCHAR(100) NOT NULL,
        Route            NVARCHAR(200) NOT NULL,
        SortOrder        INT           NOT NULL
    );
END;

-- =======================
-- Tags + TenantTags
-- =======================
IF OBJECT_ID('dbo.Tags', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Tags
    (
        TagId    INT IDENTITY(1,1) PRIMARY KEY,
        TagKey   NVARCHAR(100) NOT NULL UNIQUE,
        TagLabel NVARCHAR(100) NOT NULL
    );
END;

IF OBJECT_ID('dbo.TenantTags', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TenantTags
    (
        TenantId UNIQUEIDENTIFIER NOT NULL,
        TagId    INT             NOT NULL,
        CONSTRAINT PK_TenantTags PRIMARY KEY (TenantId, TagId)
    );
END;

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = 'FK_TenantTags_Tenants'
      AND parent_object_id = OBJECT_ID('dbo.TenantTags')
)
BEGIN
    ALTER TABLE dbo.TenantTags
        ADD CONSTRAINT FK_TenantTags_Tenants
        FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id);
END;

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = 'FK_TenantTags_Tags'
      AND parent_object_id = OBJECT_ID('dbo.TenantTags')
)
BEGIN
    ALTER TABLE dbo.TenantTags
        ADD CONSTRAINT FK_TenantTags_Tags
        FOREIGN KEY (TagId) REFERENCES dbo.Tags(Id);
END;

-- =======================
-- KycDocuments
-- =======================
IF OBJECT_ID('dbo.KycDocuments', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.KycDocuments
    (
        KycDocumentId  INT IDENTITY(1,1) PRIMARY KEY,
        IdentityUserId NVARCHAR(450) NOT NULL,
        TenantId       UNIQUEIDENTIFIER NULL,
        OrgUnitId      INT NULL,
        DocumentType   NVARCHAR(50) NOT NULL,
        FileName       NVARCHAR(200) NOT NULL,
        ReviewStatus   NVARCHAR(30) NOT NULL,
        SubmittedAtUtc DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME()
    );
END;

-- =======================
-- RemittanceTransactions
-- =======================
IF OBJECT_ID('dbo.RemittanceTransactions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RemittanceTransactions
    (
        TransactionId        INT IDENTITY(1,1) PRIMARY KEY,
        TransactionRef       NVARCHAR(50)  NOT NULL UNIQUE,
        IdentityUserId       NVARCHAR(450) NOT NULL,
        TenantId             UNIQUEIDENTIFIER NOT NULL,
        SourceOrgUnitId      INT           NOT NULL,
        DestinationOrgUnitId INT           NOT NULL,
        Amount               DECIMAL(18,2) NOT NULL,
        CurrencyCode         NVARCHAR(10)  NOT NULL,
        Status               NVARCHAR(30)  NOT NULL,
        KycStatus            NVARCHAR(30)  NOT NULL,
        CreatedAtUtc         DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME()
    );
END;

-- =======================
-- AuditLog
-- =======================
IF OBJECT_ID('dbo.AuditLog', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AuditLog
    (
        AuditLogId   BIGINT IDENTITY(1,1) PRIMARY KEY,
        ActorUserId  NVARCHAR(100)  NOT NULL,
        ActorEmail   NVARCHAR(256)  NOT NULL,
        ActionType   NVARCHAR(50)   NOT NULL,
        EntityType   NVARCHAR(50)   NOT NULL,
        EntityId     NVARCHAR(100)  NULL,
        PayloadJson  NVARCHAR(MAX)  NULL,
        IpAddress    NVARCHAR(64)   NULL,
        UserAgent    NVARCHAR(512)  NULL,
        CreatedAtUtc DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME()
    );
END;
";
            await db.ExecuteAsync(sql);
        }

        private static async Task EnsureIndexesAsync(IDbConnection db)
        {
            var sql = @"
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_AuditLog_CreatedAtUtc'
      AND object_id = OBJECT_ID('dbo.AuditLog')
)
BEGIN
    CREATE INDEX IX_AuditLog_CreatedAtUtc ON dbo.AuditLog(CreatedAtUtc DESC);
END;

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_AuditLog_ActorEmail'
      AND object_id = OBJECT_ID('dbo.AuditLog')
)
BEGIN
    CREATE INDEX IX_AuditLog_ActorEmail ON dbo.AuditLog(ActorEmail);
END;

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_AuditLog_EntityType_EntityId'
      AND object_id = OBJECT_ID('dbo.AuditLog')
)
BEGIN
    CREATE INDEX IX_AuditLog_EntityType_EntityId ON dbo.AuditLog(EntityType, EntityId);
END;
";
            await db.ExecuteAsync(sql);
        }

        private static async Task EnsureStoredProcsAsync(IDbConnection db)
        {
            var spInsert = @"
CREATE OR ALTER PROCEDURE dbo.spAuditLogInsert
    @ActorUserId NVARCHAR(100),
    @ActorEmail  NVARCHAR(256),
    @ActionType  NVARCHAR(50),
    @EntityType  NVARCHAR(50),
    @EntityId    NVARCHAR(100) = NULL,
    @PayloadJson NVARCHAR(MAX) = NULL,
    @IpAddress   NVARCHAR(64)  = NULL,
    @UserAgent   NVARCHAR(512) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.AuditLog
    (
        ActorUserId,
        ActorEmail,
        ActionType,
        EntityType,
        EntityId,
        PayloadJson,
        IpAddress,
        UserAgent
    )
    VALUES
    (
        @ActorUserId,
        @ActorEmail,
        @ActionType,
        @EntityType,
        @EntityId,
        @PayloadJson,
        @IpAddress,
        @UserAgent
    );
END;
";

            var spGetAudit = @"
CREATE OR ALTER PROCEDURE dbo.spAdminGetAuditTrail
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 500
        AuditLogId AS Id,
        ActorUserId,
        ActorEmail,
        ActionType,
        EntityType,
        EntityId,
        PayloadJson,
        IpAddress,
        UserAgent,
        CreatedAtUtc
    FROM dbo.AuditLog
    ORDER BY CreatedAtUtc DESC;
END;
";


            var spPatchTenantGeo = @"
CREATE OR ALTER PROCEDURE dbo.spPatchTenantGeo
AS
BEGIN
    SET NOCOUNT ON;

    -- Only run if columns exist
    IF COL_LENGTH('dbo.Tenants', 'CountryCode') IS NULL
        RETURN;

    UPDATE t
    SET CountryCode = v.CountryCodeVal,
        City        = v.CityVal,
        Latitude    = v.LatVal,
        Longitude   = v.LonVal
    FROM dbo.Tenants t
    JOIN (VALUES
        ('TEN-001','SO','Hargeisa',  9.5600, 44.0650),
        ('TEN-002','SO','Mogadishu', 2.0469, 45.3182),
        ('TEN-003','KE','Nairobi',  -1.2864, 36.8172),
        ('TEN-004','AE','Dubai',    25.2048, 55.2708),
        ('TEN-005','GB','London',   51.5072,-0.1276)
    ) AS v(Code, CountryCodeVal, CityVal, LatVal, LonVal)
      ON t.Code = v.Code;
END;
";

            await db.ExecuteAsync(spInsert);
            await db.ExecuteAsync(spGetAudit);
            await db.ExecuteAsync(spPatchTenantGeo);
        }
    }
}