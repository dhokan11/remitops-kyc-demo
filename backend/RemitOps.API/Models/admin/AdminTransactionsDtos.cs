namespace RemitOps.API.Models.Admin;

public class AdminTransactionListItemDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = "";
    public Guid SourceOrgUnitId { get; set; }
    public string SourceOrgUnitName { get; set; } = "";
    public Guid DestinationOrgUnitId { get; set; }
    public string DestinationOrgUnitName { get; set; } = "";
    public string SubmittedByUserId { get; set; } = "";
    public string? BeneficiaryName { get; set; }
    public string? BeneficiaryCountryCode { get; set; }
    public string? BeneficiaryCity { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "";
    public string CurrentQueue { get; set; } = "";
    public string CurrentStatus { get; set; } = "";
    public string? LastActionByUserId { get; set; }
    public string? Priority { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    public string Reference { get; set; } = "";
}