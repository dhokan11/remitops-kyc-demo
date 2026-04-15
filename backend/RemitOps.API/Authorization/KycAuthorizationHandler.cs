using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RemitOps.API.Data;

namespace RemitOps.API.Authorization;

public class KycDocumentOwnerRequirement : IAuthorizationRequirement
{
}

public class KycAuthorizationHandler : AuthorizationHandler<KycDocumentOwnerRequirement, Guid>
{
    private readonly ApplicationDbContext _context;

    public KycAuthorizationHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        KycDocumentOwnerRequirement requirement,
        Guid resource)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return;

        var document = await _context.KycDocuments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == resource);

        if (document == null)
            return;

        if (document.IdentityUserId == userId)
        {
            context.Succeed(requirement);
        }
    }
}