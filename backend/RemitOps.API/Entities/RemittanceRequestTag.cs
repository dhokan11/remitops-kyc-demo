namespace RemitOps.API.Entities;

public class RemittanceRequestTag
{
    public Guid RemittanceRequestId { get; set; }
    public Guid TagId { get; set; }
    public DateTime AddedAtUtc { get; set; } = DateTime.UtcNow;

    // Foreign keys
    public virtual RemittanceRequest RemittanceRequest { get; set; } = null!;
    public virtual Tag Tag { get; set; } = null!;
}