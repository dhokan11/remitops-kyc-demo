using Microsoft.EntityFrameworkCore;
using RemitOps.API.Data;
using RemitOps.API.Entities;
using RemitOps.API.Models.Admin;

namespace RemitOps.API.Data.Admin;

public class AdminRepository : IAdminRepository
{
    private readonly ApplicationDbContext _context;

    public AdminRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // ===== Tenants =====

    public async Task<IEnumerable<TenantListItemDto>> GetTenantsAsync()
    {
        return await _context.Tenants
            .Select(t => new TenantListItemDto
            {
                Id = t.Id,
                Name = t.Name,
                Code = t.Code,
                IsActive = t.IsActive
            })
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<TenantListItemDto?> GetTenantByIdAsync(Guid id)
    {
        return await _context.Tenants
            .Where(t => t.Id == id)
            .Select(t => new TenantListItemDto
            {
                Id = t.Id,
                Name = t.Name,
                Code = t.Code,
                IsActive = t.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<Guid> CreateTenantAsync(CreateTenantRequest request)
    {
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Code = request.Code,
            IsActive = true
        };

        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync();
        return tenant.Id;
    }

    public async Task UpdateTenantAsync(Guid id, UpdateTenantRequest request)
    {
        var tenant = await _context.Tenants.FindAsync(id);
        if (tenant == null)
            throw new InvalidOperationException($"Tenant with ID {id} not found.");

        tenant.Name = request.Name;
        tenant.Code = request.Code;
        tenant.IsActive = request.IsActive;

        _context.Tenants.Update(tenant);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteTenantAsync(Guid id)
    {
        var tenant = await _context.Tenants.FindAsync(id);
        if (tenant == null)
            throw new InvalidOperationException($"Tenant with ID {id} not found.");

        _context.Tenants.Remove(tenant);
        await _context.SaveChangesAsync();
    }

    // ===== Org Units =====

    public async Task<IEnumerable<OrgUnitListItemDto>> GetOrgUnitsAsync()
    {
        return await _context.OrgUnits
            .Include(ou => ou.Tenant)
            .Select(ou => new OrgUnitListItemDto
            {
                Id = ou.Id,
                TenantId = ou.TenantId,
                TenantName = ou.Tenant != null ? ou.Tenant.Name : "",
                Name = ou.Name,
                Code = ou.Code,
                Type = ou.Type,
                IsActive = ou.IsActive
            })
            .OrderBy(ou => ou.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<OrgUnitListItemDto>> GetOrgUnitsByTenantAsync(Guid tenantId)
    {
        return await _context.OrgUnits
            .Where(ou => ou.TenantId == tenantId)
            .Include(ou => ou.Tenant)
            .Select(ou => new OrgUnitListItemDto
            {
                Id = ou.Id,
                TenantId = ou.TenantId,
                TenantName = ou.Tenant != null ? ou.Tenant.Name : "",
                Name = ou.Name,
                Code = ou.Code,
                Type = ou.Type,
                IsActive = ou.IsActive
            })
            .OrderBy(ou => ou.Name)
            .ToListAsync();
    }

    public async Task<OrgUnitListItemDto?> GetOrgUnitByIdAsync(Guid id)
    {
        return await _context.OrgUnits
            .Where(ou => ou.Id == id)
            .Include(ou => ou.Tenant)
            .Select(ou => new OrgUnitListItemDto
            {
                Id = ou.Id,
                TenantId = ou.TenantId,
                TenantName = ou.Tenant != null ? ou.Tenant.Name : "",
                Name = ou.Name,
                Code = ou.Code,
                Type = ou.Type,
                IsActive = ou.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<Guid> CreateOrgUnitAsync(CreateOrgUnitRequest request)
    {
        var orgUnit = new OrgUnit
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            Name = request.Name,
            Code = request.Code,
            Type = request.Type,
            IsActive = true
        };

        _context.OrgUnits.Add(orgUnit);
        await _context.SaveChangesAsync();
        return orgUnit.Id;
    }

    public async Task UpdateOrgUnitAsync(Guid id, UpdateOrgUnitRequest request)
    {
        var orgUnit = await _context.OrgUnits.FindAsync(id);
        if (orgUnit == null)
            throw new InvalidOperationException($"OrgUnit with ID {id} not found.");

        orgUnit.TenantId = request.TenantId;
        orgUnit.Name = request.Name;
        orgUnit.Code = request.Code;
        orgUnit.Type = request.Type;
        orgUnit.IsActive = request.IsActive;

        _context.OrgUnits.Update(orgUnit);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteOrgUnitAsync(Guid id)
    {
        var orgUnit = await _context.OrgUnits.FindAsync(id);
        if (orgUnit == null)
            throw new InvalidOperationException($"OrgUnit with ID {id} not found.");

        _context.OrgUnits.Remove(orgUnit);
        await _context.SaveChangesAsync();
    }

    // ===== Users =====

    public async Task<IEnumerable<AdminUserListItemDto>> GetUsersAsync()
    {
        return await _context.Users
            .Select(u => new AdminUserListItemDto
            {
                Id = u.Id,
                FullName = $"{u.FirstName} {u.LastName}",
                Email = u.Email ?? "",
                Role = u.UserType,
                IsActive = u.IsActive
            })
            .OrderBy(u => u.FullName)
            .ToListAsync();
    }

    public async Task<AdminUserListItemDto?> GetUserByIdAsync(string id)
    {
        return await _context.Users
            .Where(u => u.Id == id)
            .Select(u => new AdminUserListItemDto
            {
                Id = u.Id,
                FullName = $"{u.FirstName} {u.LastName}",
                Email = u.Email ?? "",
                Role = u.UserType,
                IsActive = u.IsActive
            })
            .FirstOrDefaultAsync();
    }

public async Task<Guid> CreateUserAsync(CreateUserRequest request)
{
    // This would typically use UserManager from Identity
    // For now, return a placeholder
    await Task.CompletedTask;
    return Guid.NewGuid();
}

    public async Task UpdateUserAsync(string id, UpdateUserRequest request)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            throw new InvalidOperationException($"User with ID {id} not found.");

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Email = request.Email;
        user.IsActive = request.IsActive;
        user.TenantId = request.TenantId;
        user.OrgUnitId = request.OrgUnitId;

        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(string id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            throw new InvalidOperationException($"User with ID {id} not found.");

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }

    // ===== Dashboard =====

    public async Task<DashboardSummaryDto?> GetDashboardSummaryAsync()
    {
        var totalTenants = await _context.Tenants.CountAsync();
        var totalOrgUnits = await _context.OrgUnits.CountAsync();
        var totalUsers = await _context.Users.CountAsync();
        var totalRemittances = await _context.RemittanceRequests.CountAsync();
        var totalAmount = await _context.RemittanceRequests
            .SumAsync(r => r.Amount);

        return new DashboardSummaryDto
        {
            TotalTenants = totalTenants,
            TotalOrgUnits = totalOrgUnits,
            TotalUsers = totalUsers,
            TotalRemittanceRequests = totalRemittances,
            TotalRemittanceAmount = totalAmount
        };
    }
}