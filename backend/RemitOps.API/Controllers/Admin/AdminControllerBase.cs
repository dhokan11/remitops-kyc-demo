using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RemitOps.API.Auth;

namespace RemitOps.API.Controllers.Admin;

[ApiController]
[Authorize(Policy = Policies.ManagePlatform)]
[Route("api/admin")]
public abstract class AdminControllerBase : ControllerBase
{
    protected string ActorId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0";
    protected string ActorEmail => User.FindFirstValue(ClaimTypes.Email) ?? "unknown";
}