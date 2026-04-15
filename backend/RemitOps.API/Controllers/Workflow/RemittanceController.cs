using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RemitOps.API.Authorization;
using RemitOps.API.Data;
using RemitOps.API.Entities;
using RemitOps.API.Models.Workflow;

namespace RemitOps.API.Controllers.Workflow;

[ApiController]
[Route("api/remittances")]
[Authorize]
public class RemittanceController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IAuthorizationService _authorizationService;

    public RemittanceController(ApplicationDbContext db, IAuthorizationService authorizationService)
    {
        _db = db;
        _authorizationService = authorizationService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateRemittanceRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var tenantId = User.FindFirst("tenant_id")?.Value;

        if (userId is null || string.IsNullOrWhiteSpace(tenantId))
            return Unauthorized();

        var entity = new RemittanceRequest
        {
            TenantId = Guid.Parse(tenantId),
            SourceOrgUnitId = request.SourceOrgUnitId,
            DestinationOrgUnitId = request.DestinationOrgUnitId,
            SubmittedByUserId = userId,
            BeneficiaryName = request.BeneficiaryName,
            Amount = request.Amount,
            CurrentQueue = "SourceReview",
            CurrentStatus = "Submitted"
        };

        _db.RemittanceRequests.Add(entity);
        await _db.SaveChangesAsync();

        return Ok(entity);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var entity = await _db.RemittanceRequests.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null) return NotFound();

        var auth = await _authorizationService.AuthorizeAsync(User, entity, WorkflowOperations.View);
        if (!auth.Succeeded) return Forbid();

        return Ok(entity);
    }

    [HttpPost("{id:guid}/source-action")]
    public async Task<IActionResult> SourceAction(Guid id, QueueActionRequest request)
    {
        var entity = await _db.RemittanceRequests.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null) return NotFound();

        var auth = await _authorizationService.AuthorizeAsync(User, entity, WorkflowOperations.ActOnSourceQueue);
        if (!auth.Succeeded) return Forbid();

        entity.LastActionByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        entity.UpdatedAtUtc = DateTime.UtcNow;

        switch (request.Action)
        {
            case "Approve":
                entity.CurrentQueue = "DestinationReview";
                entity.CurrentStatus = "ReadyForDestination";
                break;
            case "KycHold":
                entity.CurrentQueue = "SourceKycHold";
                entity.CurrentStatus = "OnHold";
                break;
            case "Reject":
                entity.CurrentQueue = "Rejected";
                entity.CurrentStatus = "Rejected";
                break;
            default:
                return BadRequest(new { message = "Unsupported source action." });
        }

        await _db.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpPost("{id:guid}/destination-action")]
    public async Task<IActionResult> DestinationAction(Guid id, QueueActionRequest request)
    {
        var entity = await _db.RemittanceRequests.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null) return NotFound();

        var auth = await _authorizationService.AuthorizeAsync(User, entity, WorkflowOperations.ActOnDestinationQueue);
        if (!auth.Succeeded) return Forbid();

        entity.LastActionByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        entity.UpdatedAtUtc = DateTime.UtcNow;

        switch (request.Action)
        {
            case "ReadyForPickup":
                entity.CurrentQueue = "ReadyForPickup";
                entity.CurrentStatus = "ReadyForPickup";
                break;
            case "PaidOut":
                entity.CurrentQueue = "PaidOut";
                entity.CurrentStatus = "PaidOut";
                break;
            case "Reject":
                entity.CurrentQueue = "Rejected";
                entity.CurrentStatus = "Rejected";
                break;
            default:
                return BadRequest(new { message = "Unsupported destination action." });
        }

        await _db.SaveChangesAsync();
        return Ok(entity);
    }
}