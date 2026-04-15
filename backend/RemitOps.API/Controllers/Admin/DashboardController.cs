using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RemitOps.API.Services.Admin;

namespace RemitOps.API.Controllers.Admin;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Roles = "PlatformAdmin")]
public class DashboardController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        IAdminService adminService,
        ILogger<DashboardController> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetSummary()
    {
        try
        {
            var summary = await _adminService.GetDashboardSummaryAsync();
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving admin dashboard summary");
            return StatusCode(500, new { message = "Failed to load dashboard summary." });
        }
    }
}