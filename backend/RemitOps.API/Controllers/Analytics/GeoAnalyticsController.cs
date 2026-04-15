using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RemitOps.API.Data.Geo;

namespace RemitOps.API.Controllers.Analytics;

[ApiController]
[Route("api/analytics/geo")]
[Authorize]
public class GeoAnalyticsController : ControllerBase
{
    private readonly IGeoAnalyticsRepository _repo;

    public GeoAnalyticsController(IGeoAnalyticsRepository repo)
    {
        _repo = repo;
    }

    [HttpGet("distribution/{tenantId:guid}")]
    public async Task<IActionResult> GetGeoDistribution(Guid tenantId)
    {
        var distribution = await _repo.GetGeoDistributionAsync(tenantId);
        return Ok(distribution);
    }

    [HttpGet("distribution/{tenantId:guid}/country/{countryCode}")]
    public async Task<IActionResult> GetCountryDistribution(Guid tenantId, string countryCode)
    {
        var distribution = await _repo.GetGeoDistributionByCountryAsync(tenantId, countryCode);
        return Ok(distribution);
    }

    [HttpGet("tags")]
    public async Task<IActionResult> GetAllTags()
    {
        var tags = await _repo.GetAllTagsAsync();
        return Ok(tags);
    }

    [HttpGet("tags/remittance/{remittanceId:guid}")]
    public async Task<IActionResult> GetRemittanceTags(Guid remittanceId)
    {
        var tags = await _repo.GetTagsByRemittanceAsync(remittanceId);
        return Ok(tags);
    }

    [HttpGet("tags/org-unit/{orgUnitId:guid}")]
    public async Task<IActionResult> GetOrgUnitTags(Guid orgUnitId)
    {
        var tags = await _repo.GetTagsByOrgUnitAsync(orgUnitId);
        return Ok(tags);
    }
}