using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RemitOps.API.Auth;
using RemitOps.API.Models.Admin;
using RemitOps.API.Services.Admin;

namespace RemitOps.API.Controllers.Admin;

[Route("api/admin/users")]
[Authorize(Policy = Policies.ManageOrgUnitUsers)]
public class UsersController : AdminControllerBase
{
    private readonly IAdminService _service;

    public UsersController(IAdminService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _service.GetUsersAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var user = await _service.GetUserByIdAsync(id);
        if (user == null)
            return NotFound();

        return Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        var id = await _service.CreateUserAsync(ActorId, ActorEmail, request);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateUserRequest request)
    {
        await _service.UpdateUserAsync(ActorId, ActorEmail, id, request);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _service.DeleteUserAsync(ActorId, ActorEmail, id);
        return NoContent();
    }
}