namespace RemitOps.API.Models
{

    public record UpdateTenantRequest(string Name, string Code, string Status, string? Description);

    public record UpdateOrgUnitRequest(int TenantId, string Name, string Code, string Country, string City, string Status);

    public record CreateUserRequest(int TenantId, int? OrgUnitId, string FullName, string Email, string Role, string Password);
    public record UpdateUserRequest(int TenantId, int? OrgUnitId, string FullName, string Email, string Role, string Status);
}