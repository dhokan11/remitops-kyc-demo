namespace RemitOps.API.Entities;

public class RemittanceRequest
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid SourceOrgUnitId { get; set; }
    public Guid DestinationOrgUnitId { get; set; }
    public string SubmittedByUserId { get; set; } = "";
    public string? BeneficiaryName { get; set; }
    public string? BeneficiaryCountryCode { get; set; }
    public string? BeneficiaryCity { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string CurrentQueue { get; set; } = "Pending";
    public string CurrentStatus { get; set; } = "Submitted";
    public string? LastActionByUserId { get; set; }
    public string? Priority { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    // Foreign keys
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual OrgUnit SourceOrgUnit { get; set; } = null!;
    public virtual OrgUnit DestinationOrgUnit { get; set; } = null!;
    public virtual ApplicationUser SubmittedByUser { get; set; } = null!;
    public virtual ApplicationUser? LastActionByUser { get; set; }

    // Navigation properties
    public virtual ICollection<RemittanceAudit> AuditTrail { get; set; } = new List<RemittanceAudit>();
    public virtual ICollection<RemittanceRequestTag> Tags { get; set; } = new List<RemittanceRequestTag>();
}