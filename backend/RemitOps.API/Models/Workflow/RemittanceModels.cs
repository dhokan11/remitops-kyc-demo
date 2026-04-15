namespace RemitOps.API.Models.Workflow;

public record CreateRemittanceRequest(
    Guid SourceOrgUnitId,
    Guid DestinationOrgUnitId,
    string BeneficiaryName,
    decimal Amount
);

public record QueueActionRequest(
    string Action,
    string? Notes
);