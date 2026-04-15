using RemitOps.API.Data.Remittance;
using RemitOps.API.Models.Remittance;

namespace RemitOps.API.Services.Remittance;

public class RemittanceService : IRemittanceService
{
    private readonly IRemittanceRepository _repo;
    private readonly IAuditService _audit;

    public RemittanceService(IRemittanceRepository repo, IAuditService audit)
    {
        _repo = repo;
        _audit = audit;
    }

    public Task<IEnumerable<RemittanceListItemDto>> GetRemittancesAsync(Guid? tenantId = null)
        => _repo.GetRemittancesAsync(tenantId);

    public Task<RemittanceDetailDto?> GetRemittanceByIdAsync(Guid id)
        => _repo.GetRemittanceByIdAsync(id);

    public async Task<Guid> CreateRemittanceAsync(string userId, string userEmail, Guid tenantId, CreateRemittanceRequest request)
    {
        var id = await _repo.CreateRemittanceAsync(tenantId, userId, request);
        await _audit.LogAsync(userId, userEmail, "CREATE", "REMITTANCE", id.ToString(), request);
        return id;
    }

    public async Task UpdateRemittanceStatusAsync(string userId, string userEmail, Guid id, UpdateRemittanceStatusRequest request)
    {
        await _repo.UpdateRemittanceStatusAsync(id, userId, request);
        await _audit.LogAsync(userId, userEmail, "UPDATE_STATUS", "REMITTANCE", id.ToString(), request);
    }

    public Task<IEnumerable<RemittanceAuditDto>> GetAuditTrailAsync(Guid remittanceId)
        => _repo.GetRemittanceAuditTrailAsync(remittanceId);
}
