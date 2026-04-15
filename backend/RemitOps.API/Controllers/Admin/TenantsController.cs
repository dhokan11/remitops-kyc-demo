using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RemitOps.API.Auth;
using RemitOps.API.Models.Admin;
using RemitOps.API.Services.Admin;

namespace RemitOps.API.Controllers.Admin;

[Route("api/admin/tenants")]
[Authorize(Policy = Policies.ManageTenants)]
public class TenantsController : AdminControllerBase
{
    private readonly IAdminService _service;

    public TenantsController(IAdminService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tenants = await _service.GetTenantsAsync();
        return Ok(tenants);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var tenant = await _service.GetTenantByIdAsync(id);
        if (tenant == null)
            return NotFound();

        return Ok(tenant);
    }

[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateTenantRequest request)
{
    var tenant = await _service.CreateTenantAsync(ActorId, ActorEmail, request);
    return CreatedAtAction(nameof(GetById), new { id = tenant.Id }, tenant);
}

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTenantRequest request)
    {
        await _service.UpdateTenantAsync(ActorId, ActorEmail, id, request);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteTenantAsync(ActorId, ActorEmail, id);
        return NoContent();
    }


    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> ToggleStatus(Guid id, [FromBody] UpdateTenantStatusRequest request)
    {
        var tenant = await _service.UpdateTenantStatusAsync(ActorId, ActorEmail, id, request.IsActive);
        return Ok(tenant);
    }
}