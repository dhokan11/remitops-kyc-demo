using RemitOps.API.Models.Admin;

namespace RemitOps.API.Services.Admin;

public interface IAdminService
{
    Task<IEnumerable<TenantListItemDto>> GetTenantsAsync();
    Task<TenantListItemDto?> GetTenantByIdAsync(Guid id);
    Task<TenantListItemDto> CreateTenantAsync(string actorId, string actorEmail, CreateTenantRequest request);
    Task UpdateTenantAsync(string actorId, string actorEmail, Guid id, UpdateTenantRequest request);
    Task DeleteTenantAsync(string actorId, string actorEmail, Guid id);
    Task<TenantListItemDto> UpdateTenantStatusAsync(string actorId, string actorEmail, Guid id, bool isActive);

    Task<IEnumerable<OrgUnitListItemDto>> GetOrgUnitsAsync();
    Task<IEnumerable<OrgUnitListItemDto>> GetOrgUnitsByTenantAsync(Guid tenantId);
    Task<OrgUnitListItemDto?> GetOrgUnitByIdAsync(Guid id);
    Task<Guid> CreateOrgUnitAsync(string actorId, string actorEmail, CreateOrgUnitRequest request);
    Task UpdateOrgUnitAsync(string actorId, string actorEmail, Guid id, UpdateOrgUnitRequest request);
    Task DeleteOrgUnitAsync(string actorId, string actorEmail, Guid id);

    Task<IEnumerable<AdminUserListItemDto>> GetUsersAsync();
    Task<AdminUserListItemDto?> GetUserByIdAsync(string id);
    Task<string> CreateUserAsync(string actorId, string actorEmail, CreateUserRequest request);
    Task UpdateUserAsync(string actorId, string actorEmail, string id, UpdateUserRequest request);
    Task DeleteUserAsync(string actorId, string actorEmail, string id);

    Task<DashboardSummaryDto> GetDashboardSummaryAsync();

    Task<IEnumerable<AdminTransactionListItemDto>> GetTransactionsAsync();
    Task<AdminTransactionListItemDto?> GetTransactionByIdAsync(Guid id);

    Task<IEnumerable<AdminKycReviewDto>> GetKycCasesAsync();
}