IF DB_ID('RemitOpsDB') IS NULL
BEGIN
    CREATE DATABASE RemitOpsDB;
END
GO

USE RemitOpsDB;
GO

CREATE TABLE Tenants (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(200) NOT NULL,
    Code NVARCHAR(20) NOT NULL UNIQUE,
    Type NVARCHAR(30) NOT NULL,
    Country NVARCHAR(3) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
GO

CREATE TABLE Users (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NOT NULL REFERENCES Tenants(Id),
    Email NVARCHAR(300) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    Role NVARCHAR(50) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
GO

CREATE TABLE Transactions (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NOT NULL REFERENCES Tenants(Id),
    Reference NVARCHAR(50) NOT NULL UNIQUE,
    SenderId UNIQUEIDENTIFIER NOT NULL REFERENCES Users(Id),
    ReceiverName NVARCHAR(200) NOT NULL,
    ReceiverPhone NVARCHAR(30) NOT NULL,
    DestinationCountry NVARCHAR(3) NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Currency NVARCHAR(3) NOT NULL DEFAULT 'USD',
    Status NVARCHAR(30) NOT NULL DEFAULT 'Draft',
    RiskFlag BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
GO

CREATE INDEX IX_Transactions_Tenant_Status_Created
ON Transactions(TenantId, Status, CreatedAt);
GO

CREATE INDEX IX_Transactions_Reference
ON Transactions(Reference);
GO

CREATE OR ALTER PROCEDURE sp_DashboardSummary
    @TenantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        COUNT(*) AS TotalTx,
        SUM(CASE WHEN Status = 'Completed' THEN 1 ELSE 0 END) AS Completed,
        SUM(CASE WHEN Status = 'Failed' THEN 1 ELSE 0 END) AS Failed,
        SUM(Amount) AS TotalVolume
    FROM Transactions
    WHERE TenantId = @TenantId;
END;
GO
DECLARE @T1 UNIQUEIDENTIFIER = 'A1111111-1111-1111-1111-111111111111';
DECLARE @U1 UNIQUEIDENTIFIER = 'B1111111-1111-1111-1111-111111111111';

IF NOT EXISTS (SELECT 1 FROM Tenants WHERE Id = @T1)
BEGIN
    INSERT INTO Tenants (Id, Name, Code, Type, Country)
    VALUES (@T1, 'Dahabshiil Demo', 'DHS-DEMO', 'MTO', 'SOM');
END
GO

IF NOT EXISTS (SELECT 1 FROM Users WHERE Id = 'B1111111-1111-1111-1111-111111111111')
BEGIN
    INSERT INTO Users (Id, TenantId, Email, PasswordHash, Role)
    VALUES (
        'B1111111-1111-1111-1111-111111111111',
        'A1111111-1111-1111-1111-111111111111',
        'admin@demo.com',
        'demo_hash',
        'PlatformAdmin'
    );
END
GO

DECLARE @i INT = 1;
WHILE @i <= 30
BEGIN
    INSERT INTO Transactions (
        TenantId, Reference, SenderId, ReceiverName,
        ReceiverPhone, DestinationCountry, Amount, Currency, Status, RiskFlag, CreatedAt
    )
    VALUES (
        'A1111111-1111-1111-1111-111111111111',
        'TXN' + RIGHT('0000' + CAST(@i AS VARCHAR(4)), 4),
        'B1111111-1111-1111-1111-111111111111',
        'Demo User ' + CAST(@i AS VARCHAR(10)),
        '+252611000' + CAST(@i AS VARCHAR(10)),
        'SOM',
        100 + @i,
        'USD',
        CASE WHEN @i % 5 = 0 THEN 'Failed' ELSE 'Completed' END,
        CASE WHEN @i % 9 = 0 THEN 1 ELSE 0 END,
        DATEADD(DAY, -@i, GETUTCDATE())
    );

    SET @i = @i + 1;
END
GO
