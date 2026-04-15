using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using RemitOps.API.Auth;
using RemitOps.API.Entities;

namespace RemitOps.API.Authorization;

public class RemittanceAuthorizationHandler
    : AuthorizationHandler<OperationAuthorizationRequirement, RemittanceRequest>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OperationAuthorizationRequirement requirement,
        RemittanceRequest resource)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var orgUnitId = context.User.FindFirst("org_unit_id")?.Value;
        var isPlatformAdmin = context.User.IsInRole(Roles.PlatformAdmin);
        var isOrgUnitAdmin = context.User.IsInRole(Roles.OrgUnitAdmin);
        var isEndUser = context.User.IsInRole(Roles.EndUser);

        if (isPlatformAdmin)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        if (requirement.Name == WorkflowOperations.View.Name)
        {
            if (isEndUser && userId == resource.SubmittedByUserId)
                context.Succeed(requirement);

            if (isOrgUnitAdmin &&
                (orgUnitId == resource.SourceOrgUnitId.ToString() ||
                 orgUnitId == resource.DestinationOrgUnitId.ToString()))
                context.Succeed(requirement);
        }

        if (requirement.Name == WorkflowOperations.ActOnSourceQueue.Name &&
            isOrgUnitAdmin &&
            orgUnitId == resource.SourceOrgUnitId.ToString())
        {
            context.Succeed(requirement);
        }

        if (requirement.Name == WorkflowOperations.ActOnDestinationQueue.Name &&
            isOrgUnitAdmin &&
            orgUnitId == resource.DestinationOrgUnitId.ToString())
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}