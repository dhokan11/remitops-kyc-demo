namespace RemitOps.API.Entities;

public class KycDocument
{
    public Guid Id { get; set; }

    public string IdentityUserId { get; set; } = "";
    public Guid TenantId { get; set; }
    public Guid OrgUnitId { get; set; }

    public string DocumentType { get; set; } = "";
    public string FileName { get; set; } = "";
    public string ReviewStatus { get; set; } = "Pending";

    public virtual Tenant Tenant { get; set; } = null!;
    public virtual OrgUnit OrgUnit { get; set; } = null!;
    public virtual ApplicationUser User { get; set; } = null!;
}