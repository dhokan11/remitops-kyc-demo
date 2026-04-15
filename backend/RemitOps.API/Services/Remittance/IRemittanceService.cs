using RemitOps.API.Models.Remittance;

namespace RemitOps.API.Services.Remittance;

public interface IRemittanceService
{
    Task<IEnumerable<RemittanceListItemDto>> GetRemittancesAsync(Guid? tenantId = null);
    Task<RemittanceDetailDto?> GetRemittanceByIdAsync(Guid id);
    Task<Guid> CreateRemittanceAsync(string userId, string userEmail, Guid tenantId, CreateRemittanceRequest request);
    Task UpdateRemittanceStatusAsync(string userId, string userEmail, Guid id, UpdateRemittanceStatusRequest request);
    Task<IEnumerable<RemittanceAuditDto>> GetAuditTrailAsync(Guid remittanceId);
}
