namespace RemitOps.API.Entities;

public class RemittanceAudit
{
    public Guid Id { get; set; }
    public Guid RemittanceRequestId { get; set; }
    public string Action { get; set; } = "";
    public string PerformedByUserId { get; set; } = "";
    public string Notes { get; set; } = "";
    public DateTime PerformedAtUtc { get; set; } = DateTime.UtcNow;

    public virtual RemittanceRequest RemittanceRequest { get; set; } = null!;
    public virtual ApplicationUser PerformedByUser { get; set; } = null!;
}
