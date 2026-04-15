namespace RemitOps.API.Entities;

public class OrgUnit
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid? GeoLocationId { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ContactEmail { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? CountryCode { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? TimeZone { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    // Foreign keys
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual GeoLocation? GeoLocation { get; set; }

    // Navigation properties
    public virtual ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    public virtual ICollection<RemittanceRequest> SourceRemittances { get; set; } = new List<RemittanceRequest>();
    public virtual ICollection<RemittanceRequest> DestinationRemittances { get; set; } = new List<RemittanceRequest>();
    public virtual ICollection<KycDocument> KycDocuments { get; set; } = new List<KycDocument>();
    public virtual ICollection<OrgUnitTag> Tags { get; set; } = new List<OrgUnitTag>();
}