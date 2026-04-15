using System;
using System.Collections.Generic;

namespace RemitOps.API.Entities
{
    public class Tenant
    {
        public Guid Id { get; set; }

        public string Code { get; set; } = "";
        public string Name { get; set; } = "";

        // Geo / location attributes for charts and maps
        public string? CountryCode { get; set; }
        public string? City { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        // Flags / status
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }

        // Navigation properties
        public virtual ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        public virtual ICollection<OrgUnit> OrgUnits { get; set; } = new List<OrgUnit>();
        public virtual ICollection<RemittanceRequest> RemittanceRequests { get; set; } = new List<RemittanceRequest>();
    }
}