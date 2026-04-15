namespace RemitOps.API.Models.Remittance;

public class RemittanceListItemDto
{
    public Guid Id { get; set; }
    public string BeneficiaryName { get; set; } = "";
    public decimal Amount { get; set; }
    public string SourceOrgUnitName { get; set; } = "";
    public string DestinationOrgUnitName { get; set; } = "";
    public string CurrentStatus { get; set; } = "";
    public string CurrentQueue { get; set; } = "";
    public DateTime CreatedAtUtc { get; set; }
}

public class RemittanceDetailDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid SourceOrgUnitId { get; set; }
    public Guid DestinationOrgUnitId { get; set; }
    public string SourceOrgUnitName { get; set; } = "";
    public string DestinationOrgUnitName { get; set; } = "";
    public string SubmittedByEmail { get; set; } = "";
    public string? BeneficiaryName { get; set; }
    public decimal Amount { get; set; }
    public string CurrentQueue { get; set; } = "";
    public string CurrentStatus { get; set; } = "";
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public List<RemittanceAuditDto> AuditTrail { get; set; } = new();
}

public class RemittanceAuditDto
{
    public Guid Id { get; set; }
    public string Action { get; set; } = "";
    public string PerformedByEmail { get; set; } = "";
    public string Notes { get; set; } = "";
    public DateTime PerformedAtUtc { get; set; }
}

public class CreateRemittanceRequest
{
    public Guid SourceOrgUnitId { get; set; }
    public Guid DestinationOrgUnitId { get; set; }
    public string? BeneficiaryName { get; set; }
    public decimal Amount { get; set; }
}

public class UpdateRemittanceStatusRequest
{
    public string NewStatus { get; set; } = "";
    public string Notes { get; set; } = "";
}
