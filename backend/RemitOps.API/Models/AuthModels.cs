namespace RemitOps.API.Models;

public record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string UserType // Individual or Corporate
);

public record LoginRequest(
    string Email,
    string Password
);

public record CreateTenantRequest(
    string Code,
    string Name
);

public record CreateOrgUnitRequest(
    Guid TenantId,
    string Code,
    string Name,
    string Type
);

public record CreateOrgUnitAdminRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    Guid TenantId,
    Guid OrgUnitId
);