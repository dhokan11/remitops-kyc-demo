namespace RemitOps.API.Models.Admin;

public class AdminKycReviewDto
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = "";

    public Guid OrgUnitId { get; set; }
    public string OrgUnitName { get; set; } = "";

    public string IdentityUserId { get; set; } = "";
    public string ApplicantName { get; set; } = "";
    public string ApplicantEmail { get; set; } = "";

    public string DocumentType { get; set; } = "";
    public string FileName { get; set; } = "";

    public string Status { get; set; } = "";
    public string? SubmittedAtUtc { get; set; }
}