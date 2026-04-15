using Microsoft.EntityFrameworkCore;
using RemitOps.API.Data;
using RemitOps.API.Models.Filtering;

namespace RemitOps.API.Data.Geo;

public class GeoAnalyticsRepository : IGeoAnalyticsRepository
{
    private readonly ApplicationDbContext _context;

    public GeoAnalyticsRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<GeoDistributionDto>> GetGeoDistributionAsync(Guid tenantId)
    {
        var distribution = await _context.OrgUnits
            .Where(ou => ou.TenantId == tenantId && ou.IsActive)
            .GroupBy(ou => new { ou.CountryCode, ou.GeoLocation!.CountryName, ou.Latitude, ou.Longitude })
            .Select(g => new
            {
                g.Key.CountryCode,
                g.Key.CountryName,
                g.Key.Latitude,
                g.Key.Longitude,
                OrgUnitCount = g.Count(),
                OrgUnitIds = g.Select(ou => ou.Id).ToList()
            })
            .ToListAsync();

        var result = new List<GeoDistributionDto>();

        foreach (var country in distribution)
        {
            var remittances = await _context.RemittanceRequests
                .Where(r => r.TenantId == tenantId && 
                           (r.SourceOrgUnit.CountryCode == country.CountryCode || 
                            r.DestinationOrgUnit.CountryCode == country.CountryCode))
                .ToListAsync();

            var cities = await _context.OrgUnits
                .Where(ou => ou.CountryCode == country.CountryCode && ou.TenantId == tenantId)
                .GroupBy(ou => new { ou.City, ou.State, ou.Latitude, ou.Longitude })
                .Select(g => new CityDistributionDto
                {
                    City = g.Key.City ?? "",
                    State = g.Key.State ?? "",
                    Latitude = g.Key.Latitude ?? 0,
                    Longitude = g.Key.Longitude ?? 0,
                    OrgUnits = g.Count()
                })
                .ToListAsync();

            var geoDto = new GeoDistributionDto
            {
                CountryCode = country.CountryCode ?? "",
                CountryName = country.CountryName ?? "",
                Latitude = country.Latitude ?? 0,
                Longitude = country.Longitude ?? 0,
                TotalOrgUnits = country.OrgUnitCount,
                TotalRemittances = remittances.Count,
                TotalAmount = remittances.Sum(r => r.Amount),
                Cities = cities
            };

            foreach (var city in geoDto.Cities)
            {
                city.Remittances = remittances
                    .Where(r => (r.SourceOrgUnit.City == city.City || r.DestinationOrgUnit.City == city.City))
                    .Count();
                city.Amount = remittances
                    .Where(r => (r.SourceOrgUnit.City == city.City || r.DestinationOrgUnit.City == city.City))
                    .Sum(r => r.Amount);
            }

            result.Add(geoDto);
        }

        return result;
    }

    public async Task<List<GeoDistributionDto>> GetGeoDistributionByCountryAsync(Guid tenantId, string countryCode)
    {
        return (await GetGeoDistributionAsync(tenantId))
            .Where(g => g.CountryCode == countryCode)
            .ToList();
    }

    public async Task<List<TagDto>> GetAllTagsAsync()
    {
        return await _context.Tags
            .Where(t => t.IsActive)
            .Select(t => new TagDto
            {
                Id = t.Id,
                Name = t.Name,
                Category = t.Category,
                Color = t.Color,
                Description = t.Description,
                UsageCount = t.OrgUnitTags.Count + t.RemittanceRequestTags.Count
            })
            .OrderBy(t => t.Category)
            .ThenBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<List<TagDto>> GetTagsByRemittanceAsync(Guid remittanceId)
    {
        return await _context.RemittanceRequestTags
            .Where(rt => rt.RemittanceRequestId == remittanceId)
            .Select(rt => new TagDto
            {
                Id = rt.Tag.Id,
                Name = rt.Tag.Name,
                Category = rt.Tag.Category,
                Color = rt.Tag.Color,
                Description = rt.Tag.Description,
                UsageCount = rt.Tag.OrgUnitTags.Count + rt.Tag.RemittanceRequestTags.Count
            })
            .ToListAsync();
    }

    public async Task<List<TagDto>> GetTagsByOrgUnitAsync(Guid orgUnitId)
    {
        return await _context.OrgUnitTags
            .Where(ot => ot.OrgUnitId == orgUnitId)
            .Select(ot => new TagDto
            {
                Id = ot.Tag.Id,
                Name = ot.Tag.Name,
                Category = ot.Tag.Category,
                Color = ot.Tag.Color,
                Description = ot.Tag.Description,
                UsageCount = ot.Tag.OrgUnitTags.Count + ot.Tag.RemittanceRequestTags.Count
            })
            .ToListAsync();
    }
}