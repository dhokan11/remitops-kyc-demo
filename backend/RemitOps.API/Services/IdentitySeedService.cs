using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using RemitOps.API.Auth;
using RemitOps.API.Entities;
using RemitOps.API.Models;

namespace RemitOps.API.Services
{
    public class IdentitySeedService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SeedAdminOptions _seedAdmin;

        public IdentitySeedService(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            IOptions<SeedAdminOptions> seedAdmin)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _seedAdmin = seedAdmin.Value;
        }

        public async Task SeedAsync()
        {
            var roles = new[]
            {
                Roles.PlatformAdmin,
                Roles.OrgUnitAdmin,
                Roles.EndUser
            };

            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var admin = await _userManager.FindByEmailAsync(_seedAdmin.Email);

            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = _seedAdmin.UserName,
                    Email = _seedAdmin.Email,
                    FirstName = _seedAdmin.FirstName,
                    LastName = _seedAdmin.LastName,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(admin, _seedAdmin.Password);

                if (!result.Succeeded)
                    throw new InvalidOperationException(
                        string.Join("; ", result.Errors.Select(e => e.Description)));
            }

            if (!await _userManager.IsInRoleAsync(admin, Roles.PlatformAdmin))
            {
                await _userManager.AddToRoleAsync(admin, Roles.PlatformAdmin);
            }
        }
    }
}