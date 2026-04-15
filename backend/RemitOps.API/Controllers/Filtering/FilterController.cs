using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RemitOps.API.Data.Remittance;
using RemitOps.API.Models.Filtering;

namespace RemitOps.API.Controllers.Filtering;

[ApiController]
[Route("api/filter")]
[Authorize]
public class FilterController : ControllerBase
{
    private readonly IRemittanceRepository _repo;

    public FilterController(IRemittanceRepository repo)
    {
        _repo = repo;
    }

    [HttpPost("remittances")]
    public async Task<IActionResult> FilterRemittances([FromBody] RemittanceFilterRequest filter)
    {
        var result = await _repo.FilterRemittancesAsync(filter);
        return Ok(result);
    }
}