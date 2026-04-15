using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RemitOps.API.Services.Admin;

namespace RemitOps.API.Controllers;

[ApiController]
[Route("api/admin/transactions")]
[Authorize(Roles = "PlatformAdmin")]
public class AdminTransactionsController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly ILogger<AdminTransactionsController> _logger;

    public AdminTransactionsController(
        IAdminService adminService,
        ILogger<AdminTransactionsController> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetTransactions()
    {
        var rows = await _adminService.GetTransactionsAsync();
        return Ok(rows);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTransactionById(Guid id)
    {
        var row = await _adminService.GetTransactionByIdAsync(id);
        if (row == null) return NotFound();

        return Ok(row);
    }
}