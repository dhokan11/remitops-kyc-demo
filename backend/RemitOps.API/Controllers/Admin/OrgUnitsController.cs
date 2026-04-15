using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RemitOps.API.Auth;
using RemitOps.API.Models.Admin;
using RemitOps.API.Services.Admin;

namespace RemitOps.API.Controllers.Admin;

[Route("api/admin/org-units")]
[Authorize(Policy = Policies.ManageOrgUnits)]
public class OrgUnitsController : AdminControllerBase
{
    private readonly IAdminService _service;

    public OrgUnitsController(IAdminService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var orgUnits = await _service.GetOrgUnitsAsync();
        return Ok(orgUnits);
    }

    [HttpGet("by-tenant/{tenantId:guid}")]
    public async Task<IActionResult> GetByTenant(Guid tenantId)
    {
        var orgUnits = await _service.GetOrgUnitsByTenantAsync(tenantId);
        return Ok(orgUnits);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var orgUnit = await _service.GetOrgUnitByIdAsync(id);
        if (orgUnit == null)
            return NotFound();

        return Ok(orgUnit);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrgUnitRequest request)
    {
        var id = await _service.CreateOrgUnitAsync(ActorId, ActorEmail, request);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOrgUnitRequest request)
    {
        await _service.UpdateOrgUnitAsync(ActorId, ActorEmail, id, request);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteOrgUnitAsync(ActorId, ActorEmail, id);
        return NoContent();
    }
}