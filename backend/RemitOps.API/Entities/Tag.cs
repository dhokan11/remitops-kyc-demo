namespace RemitOps.API.Entities;

public class Tag
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string Color { get; set; } = "#000000";
    public string Description { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<OrgUnitTag> OrgUnitTags { get; set; } = new List<OrgUnitTag>();
    public virtual ICollection<RemittanceRequestTag> RemittanceRequestTags { get; set; } = new List<RemittanceRequestTag>();
}