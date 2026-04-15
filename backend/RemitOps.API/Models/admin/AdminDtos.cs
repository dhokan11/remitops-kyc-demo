namespace RemitOps.API.Models.Admin;

public class TenantListItemDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";

    public string? CountryCode { get; set; }
    public string? City { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }

    public bool IsActive { get; set; }
    public string? Status { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}

public class CreateTenantRequest
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";

    public string? CountryCode { get; set; }
    public string? City { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
}

public class UpdateTenantRequest
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";

    public string? CountryCode { get; set; }
    public string? City { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }

    public bool IsActive { get; set; }
}

public class UpdateTenantStatusRequest
{
    public bool IsActive { get; set; }
}


public class OrgUnitListItemDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = "";
    public string Name { get; set; } = "";
    public string Code { get; set; } = "";
    public string Type { get; set; } = "";
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class CreateOrgUnitRequest
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = "";
    public string Code { get; set; } = "";
    public string Type { get; set; } = "";
}

public class UpdateOrgUnitRequest
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = "";
    public string Code { get; set; } = "";
    public string Type { get; set; } = "";
    public bool IsActive { get; set; }
}

public class AdminUserListItemDto
{
    public string Id { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "";
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class CreateUserRequest
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string Role { get; set; } = "";
    public Guid? TenantId { get; set; }
    public Guid? OrgUnitId { get; set; }
}

public class UpdateUserRequest
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "";
    public bool IsActive { get; set; }
    public Guid? TenantId { get; set; }
    public Guid? OrgUnitId { get; set; }
}


