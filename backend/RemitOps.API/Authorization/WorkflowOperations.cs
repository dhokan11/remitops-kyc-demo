using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace RemitOps.API.Authorization;

public static class WorkflowOperations
{
    public static OperationAuthorizationRequirement View =
        new() { Name = nameof(View) };

    public static OperationAuthorizationRequirement ActOnSourceQueue =
        new() { Name = nameof(ActOnSourceQueue) };

    public static OperationAuthorizationRequirement ActOnDestinationQueue =
        new() { Name = nameof(ActOnDestinationQueue) };

    public static OperationAuthorizationRequirement ReviewKyc =
        new() { Name = nameof(ReviewKyc) };
}