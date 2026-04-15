namespace RemitOps.API.Entities;

public class GeoLocation
{
    public Guid Id { get; set; }
    public string CountryCode { get; set; } = "";
    public string CountryName { get; set; } = "";
    public string City { get; set; } = "";
    public string State { get; set; } = "";
    public string PostalCode { get; set; } = "";
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string TimeZone { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<OrgUnit> OrgUnits { get; set; } = new List<OrgUnit>();
}