namespace RemitOps.API.Entities;

public class OrgUnitTag
{
    public Guid OrgUnitId { get; set; }
    public Guid TagId { get; set; }
    public DateTime AddedAtUtc { get; set; } = DateTime.UtcNow;

    // Foreign keys
    public virtual OrgUnit OrgUnit { get; set; } = null!;
    public virtual Tag Tag { get; set; } = null!;
}