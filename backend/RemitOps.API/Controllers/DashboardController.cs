using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using RemitOps.API.Models;

namespace RemitOps.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        IConfiguration configuration,
        ILogger<DashboardController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet("summary/{tenantId}")]
    public async Task<IActionResult> GetSummary(Guid tenantId)
    {
        _logger.LogTrace("Entered GetSummary with tenantId {TenantId}", tenantId);
        _logger.LogInformation("Dashboard summary requested for tenant {TenantId}", tenantId);

        using var connection = new SqlConnection(
            _configuration.GetConnectionString("DefaultConnection"));

        _logger.LogDebug("Opening SQL connection using DefaultConnection");

        var result = await connection.QueryFirstOrDefaultAsync<DashboardSummaryDto>(
            "sp_DashboardSummary",
            new { TenantId = tenantId },
            commandType: CommandType.StoredProcedure);

        if (result == null)
        {
            _logger.LogWarning("No dashboard summary returned for tenant {TenantId}", tenantId);
            return NotFound();
        }

        _logger.LogInformation(
            "Dashboard summary returned for tenant {TenantId}. TotalTx={TotalTx}, Completed={Completed}, Failed={Failed}, TotalVolume={TotalVolume}",
            tenantId, result.TotalTx, result.Completed, result.Failed, result.TotalVolume);

        return Ok(result);
    }
}