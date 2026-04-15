namespace RemitOps.API.Models.Workflow;

public record ReviewKycRequest(
    string Action,   // Verify, Reject, NeedsReupload
    string? Notes
);