using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RemitOps.API.Data;
using RemitOps.API.Entities;

namespace RemitOps.API.Controllers.Workflow;

[ApiController]
[Route("api/workflow/kyc")]
[Authorize]
public class WorkflowKycController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<WorkflowKycController> _logger;

    public WorkflowKycController(
        ApplicationDbContext context,
        ILogger<WorkflowKycController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] WorkflowCreateKycDocumentRequest request)
    {
        try
        {
            var document = new KycDocument
            {
                Id = Guid.NewGuid(),
                IdentityUserId = request.IdentityUserId,
                TenantId = request.TenantId,
                OrgUnitId = request.OrgUnitId,
                DocumentType = request.DocumentType,
                FileName = request.FileName,
                ReviewStatus = string.IsNullOrWhiteSpace(request.ReviewStatus)
                    ? "Pending"
                    : request.ReviewStatus
            };

            _context.KycDocuments.Add(document);
            await _context.SaveChangesAsync();

            return Ok(document);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating workflow KYC document");
            return StatusCode(500, new { message = "Failed to create KYC document." });
        }
    }
}

public class WorkflowCreateKycDocumentRequest
{
    public string IdentityUserId { get; set; } = "";
    public Guid TenantId { get; set; }
    public Guid OrgUnitId { get; set; }
    public string DocumentType { get; set; } = "";
    public string FileName { get; set; } = "";
    public string? ReviewStatus { get; set; }
}