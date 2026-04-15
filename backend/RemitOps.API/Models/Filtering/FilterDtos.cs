namespace RemitOps.API.Models.Filtering;

// ===== PAGINATION RESULT =====

public class PaginatedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}

// ===== REMITTANCE FILTERING =====

public class RemittanceFilterRequest
{
    public Guid? TenantId { get; set; }
    public Guid? SourceOrgUnitId { get; set; }
    public Guid? DestinationOrgUnitId { get; set; }
    public string? Status { get; set; }
    public string? Queue { get; set; }
    public string? Priority { get; set; }
    public string? Currency { get; set; }
    public string? BeneficiaryCountryCode { get; set; }
    public string? SourceCountryCode { get; set; }
    public string? DestinationCountryCode { get; set; }
    public string? SourceCity { get; set; }
    public string? DestinationCity { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<string>? Tags { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string? SortBy { get; set; } = "CreatedAtUtc";
    public bool SortDescending { get; set; } = true;
}

// ===== GEO DISTRIBUTION & ANALYTICS =====

public class GeoDistributionDto
{
    public string CountryCode { get; set; } = "";
    public string CountryName { get; set; } = "";
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public int TotalOrgUnits { get; set; }
    public int TotalRemittances { get; set; }
    public decimal TotalAmount { get; set; }
    public List<CityDistributionDto> Cities { get; set; } = new();
}

public class CityDistributionDto
{
    public string City { get; set; } = "";
    public string State { get; set; } = "";
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public int OrgUnits { get; set; }
    public int Remittances { get; set; }
    public decimal Amount { get; set; }
}

// ===== TAG DTO =====

public class TagDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string Color { get; set; } = "";
    public string Description { get; set; } = "";
    public int UsageCount { get; set; }
}

// ===== ORG UNIT FILTERING =====

public class OrgUnitFilterRequest
{
    public Guid? TenantId { get; set; }
    public string? Type { get; set; }
    public string? CountryCode { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public List<string>? Tags { get; set; }
    public bool? IsActive { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string? SortBy { get; set; } = "Name";
    public bool SortDescending { get; set; } = false;
}
