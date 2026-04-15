using RemitOps.API.Models.Filtering;
using RemitOps.API.Models.Remittance;

namespace RemitOps.API.Data.Remittance;

public interface IRemittanceRepository
{
    Task<IEnumerable<RemittanceListItemDto>> GetRemittancesAsync(Guid? tenantId = null);
    Task<RemittanceDetailDto?> GetRemittanceByIdAsync(Guid id);
    Task<Guid> CreateRemittanceAsync(Guid tenantId, string userId, CreateRemittanceRequest request);
    Task UpdateRemittanceStatusAsync(Guid id, string userId, UpdateRemittanceStatusRequest request);
    Task<IEnumerable<RemittanceAuditDto>> GetRemittanceAuditTrailAsync(Guid remittanceId);
    Task<PaginatedResult<RemittanceListItemDto>> FilterRemittancesAsync(RemittanceFilterRequest filter);
    Task<List<RemittanceCorridorDto>> GetRemittanceCorridorsAsync(Guid tenantId);
    Task<RemittanceSummaryStatsDto> GetRemittanceSummaryStatsAsync(Guid tenantId);
}
