using Microsoft.AspNetCore.Identity;

namespace RemitOps.API.Entities;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    public Guid? TenantId { get; set; }
    public Guid? OrgUnitId { get; set; }
    public Guid? RemitterId { get; set; }

    public string UserType { get; set; } = "EndUser";
    public string RegistrationStatus { get; set; } = "PendingEmailConfirmation";
    public bool IsActive { get; set; } = true;

    public Tenant? Tenant { get; set; }
    public OrgUnit? OrgUnit { get; set; }
}