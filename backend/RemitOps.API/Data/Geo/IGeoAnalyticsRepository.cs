using RemitOps.API.Models.Filtering;

namespace RemitOps.API.Data.Geo;

public interface IGeoAnalyticsRepository
{
    Task<List<GeoDistributionDto>> GetGeoDistributionAsync(Guid tenantId);
    Task<List<GeoDistributionDto>> GetGeoDistributionByCountryAsync(Guid tenantId, string countryCode);
    Task<List<TagDto>> GetAllTagsAsync();
    Task<List<TagDto>> GetTagsByRemittanceAsync(Guid remittanceId);
    Task<List<TagDto>> GetTagsByOrgUnitAsync(Guid orgUnitId);
}