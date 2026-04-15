using RemitOps.API.Models.Admin;

namespace RemitOps.API.Data.Admin;

public interface IAdminRepository
{
    // Tenants
    Task<IEnumerable<TenantListItemDto>> GetTenantsAsync();
    Task<TenantListItemDto?> GetTenantByIdAsync(Guid id);
    Task<Guid> CreateTenantAsync(CreateTenantRequest request);
    Task UpdateTenantAsync(Guid id, UpdateTenantRequest request);
    Task DeleteTenantAsync(Guid id);

    // Org Units
    Task<IEnumerable<OrgUnitListItemDto>> GetOrgUnitsAsync();
    Task<IEnumerable<OrgUnitListItemDto>> GetOrgUnitsByTenantAsync(Guid tenantId);
    Task<OrgUnitListItemDto?> GetOrgUnitByIdAsync(Guid id);
    Task<Guid> CreateOrgUnitAsync(CreateOrgUnitRequest request);
    Task UpdateOrgUnitAsync(Guid id, UpdateOrgUnitRequest request);
    Task DeleteOrgUnitAsync(Guid id);

    // Users
    Task<IEnumerable<AdminUserListItemDto>> GetUsersAsync();
    Task<AdminUserListItemDto?> GetUserByIdAsync(string id);
    Task<Guid> CreateUserAsync(CreateUserRequest request);
    Task UpdateUserAsync(string id, UpdateUserRequest request);
    Task DeleteUserAsync(string id);

    // Dashboard
    Task<DashboardSummaryDto?> GetDashboardSummaryAsync();
}