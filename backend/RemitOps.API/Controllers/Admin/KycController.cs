using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RemitOps.API.Auth;
using RemitOps.API.Services.Admin;

namespace RemitOps.API.Controllers.Admin;

[ApiController]
[Route("api/admin/kyc")]
[Authorize(Policy = Policies.ManageTenants)]
public class KycController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly ILogger<KycController> _logger;

    public KycController(
        IAdminService adminService,
        ILogger<KycController> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var items = await _adminService.GetKycCasesAsync();
            return Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving KYC review queue");
            return StatusCode(500, new { message = "Failed to load KYC review queue." });
        }
    }
}