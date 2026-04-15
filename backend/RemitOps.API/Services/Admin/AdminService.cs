using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RemitOps.API.Data;
using RemitOps.API.Entities;
using RemitOps.API.Models.Admin;

namespace RemitOps.API.Services.Admin;

public class AdminService : IAdminService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AdminService> _logger;

    public AdminService(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<AdminService> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<IEnumerable<TenantListItemDto>> GetTenantsAsync()
    {
        try
        {
            var tenants = await _context.Tenants
                .OrderByDescending(t => t.CreatedAtUtc)
                .Select(t => new TenantListItemDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Code = t.Code,
                    CountryCode = t.CountryCode,
                    City = t.City,
                    Latitude = t.Latitude,
                    Longitude = t.Longitude,
                    IsActive = t.IsActive,
                    Status = t.IsActive ? "Active" : "Inactive",
                    CreatedAtUtc = t.CreatedAtUtc,
                    UpdatedAtUtc = t.UpdatedAtUtc
                })
                .ToListAsync();

            return tenants;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all tenants");
            throw;
        }
    }

    public async Task<TenantListItemDto?> GetTenantByIdAsync(Guid id)
    {
        try
        {
            var tenant = await _context.Tenants
                .Where(t => t.Id == id)
                .Select(t => new TenantListItemDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Code = t.Code,
                    CountryCode = t.CountryCode,
                    City = t.City,
                    Latitude = t.Latitude,
                    Longitude = t.Longitude,
                    IsActive = t.IsActive,
                    Status = t.IsActive ? "Active" : "Inactive",
                    CreatedAtUtc = t.CreatedAtUtc,
                    UpdatedAtUtc = t.UpdatedAtUtc
                })
                .FirstOrDefaultAsync();

            return tenant;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tenant {TenantId}", id);
            throw;
        }
    }

    public async Task<TenantListItemDto> CreateTenantAsync(string actorId, string actorEmail, CreateTenantRequest request)
    {
        try
        {
            var normalizedCode = request.Code.Trim().ToUpperInvariant();

            var existingTenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.Code == normalizedCode);

            if (existingTenant != null)
                throw new InvalidOperationException($"Tenant with code '{normalizedCode}' already exists");

            var tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = request.Name.Trim(),
                Code = normalizedCode,
                CountryCode = string.IsNullOrWhiteSpace(request.CountryCode) ? null : request.CountryCode.Trim().ToUpperInvariant(),
                City = string.IsNullOrWhiteSpace(request.City) ? null : request.City.Trim(),
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Tenant created by {ActorEmail}: {TenantId} - {TenantCode}",
                actorEmail,
                tenant.Id,
                tenant.Code);

            return new TenantListItemDto
            {
                Id = tenant.Id,
                Code = tenant.Code,
                Name = tenant.Name,
                CountryCode = tenant.CountryCode,
                City = tenant.City,
                Latitude = tenant.Latitude,
                Longitude = tenant.Longitude,
                IsActive = tenant.IsActive,
                Status = tenant.IsActive ? "Active" : "Inactive",
                CreatedAtUtc = tenant.CreatedAtUtc,
                UpdatedAtUtc = tenant.UpdatedAtUtc
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tenant by {ActorEmail}", actorEmail);
            throw;
        }
    }

    public async Task UpdateTenantAsync(string actorId, string actorEmail, Guid id, UpdateTenantRequest request)
    {
        try
        {
            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == id);
            if (tenant == null)
                throw new InvalidOperationException("Tenant not found");

            var normalizedCode = request.Code.Trim().ToUpperInvariant();

            var duplicate = await _context.Tenants
                .FirstOrDefaultAsync(t => t.Id != id && t.Code == normalizedCode);

            if (duplicate != null)
                throw new InvalidOperationException($"Tenant with code '{normalizedCode}' already exists");

            tenant.Name = request.Name.Trim();
            tenant.Code = normalizedCode;
            tenant.CountryCode = string.IsNullOrWhiteSpace(request.CountryCode) ? null : request.CountryCode.Trim().ToUpperInvariant();
            tenant.City = string.IsNullOrWhiteSpace(request.City) ? null : request.City.Trim();
            tenant.Latitude = request.Latitude;
            tenant.Longitude = request.Longitude;
            tenant.IsActive = request.IsActive;
            tenant.UpdatedAtUtc = DateTime.UtcNow;

            _context.Tenants.Update(tenant);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Tenant updated by {ActorEmail}: {TenantId}", actorEmail, tenant.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tenant {TenantId} by {ActorEmail}", id, actorEmail);
            throw;
        }
    }

    public async Task DeleteTenantAsync(string actorId, string actorEmail, Guid id)
    {
        try
        {
            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == id);
            if (tenant == null)
                throw new InvalidOperationException("Tenant not found");

            _context.Tenants.Remove(tenant);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Tenant deleted by {ActorEmail}: {TenantId}", actorEmail, tenant.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tenant {TenantId} by {ActorEmail}", id, actorEmail);
            throw;
        }
    }

    public async Task<TenantListItemDto> UpdateTenantStatusAsync(string actorId, string actorEmail, Guid id, bool isActive)
    {
        try
        {
            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == id);
            if (tenant == null)
                throw new InvalidOperationException("Tenant not found");

            tenant.IsActive = isActive;
            tenant.UpdatedAtUtc = DateTime.UtcNow;

            _context.Tenants.Update(tenant);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Tenant status updated by {ActorEmail}: {TenantId}, IsActive={IsActive}",
                actorEmail,
                tenant.Id,
                tenant.IsActive);

            return new TenantListItemDto
            {
                Id = tenant.Id,
                Name = tenant.Name,
                Code = tenant.Code,
                CountryCode = tenant.CountryCode,
                City = tenant.City,
                Latitude = tenant.Latitude,
                Longitude = tenant.Longitude,
                IsActive = tenant.IsActive,
                Status = tenant.IsActive ? "Active" : "Inactive",
                CreatedAtUtc = tenant.CreatedAtUtc,
                UpdatedAtUtc = tenant.UpdatedAtUtc
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tenant status {TenantId} by {ActorEmail}", id, actorEmail);
            throw;
        }
    }

    public async Task<IEnumerable<OrgUnitListItemDto>> GetOrgUnitsAsync()
    {
        try
        {
            var orgUnits = await _context.OrgUnits
                .Include(o => o.Tenant)
                .OrderByDescending(o => o.CreatedAtUtc)
                .Select(o => new OrgUnitListItemDto
                {
                    Id = o.Id,
                    TenantId = o.TenantId,
                    TenantName = o.Tenant != null ? o.Tenant.Name : "Unknown",
                    Name = o.Name,
                    Code = o.Code,
                    Type = o.Type,
                    IsActive = o.IsActive,
                    CreatedAtUtc = o.CreatedAtUtc
                })
                .ToListAsync();

            return orgUnits;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all org units");
            throw;
        }
    }

    public async Task<IEnumerable<OrgUnitListItemDto>> GetOrgUnitsByTenantAsync(Guid tenantId)
    {
        try
        {
            var orgUnits = await _context.OrgUnits
                .Where(o => o.TenantId == tenantId)
                .Include(o => o.Tenant)
                .OrderByDescending(o => o.CreatedAtUtc)
                .Select(o => new OrgUnitListItemDto
                {
                    Id = o.Id,
                    TenantId = o.TenantId,
                    TenantName = o.Tenant != null ? o.Tenant.Name : "Unknown",
                    Name = o.Name,
                    Code = o.Code,
                    Type = o.Type,
                    IsActive = o.IsActive,
                    CreatedAtUtc = o.CreatedAtUtc
                })
                .ToListAsync();

            return orgUnits;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving org units for tenant {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<OrgUnitListItemDto?> GetOrgUnitByIdAsync(Guid id)
    {
        try
        {
            var orgUnit = await _context.OrgUnits
                .Include(o => o.Tenant)
                .Where(o => o.Id == id)
                .Select(o => new OrgUnitListItemDto
                {
                    Id = o.Id,
                    TenantId = o.TenantId,
                    TenantName = o.Tenant != null ? o.Tenant.Name : "Unknown",
                    Name = o.Name,
                    Code = o.Code,
                    Type = o.Type,
                    IsActive = o.IsActive,
                    CreatedAtUtc = o.CreatedAtUtc
                })
                .FirstOrDefaultAsync();

            return orgUnit;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving org unit {OrgUnitId}", id);
            throw;
        }
    }

    public async Task<Guid> CreateOrgUnitAsync(string actorId, string actorEmail, CreateOrgUnitRequest request)
    {
        try
        {
            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == request.TenantId);
            if (tenant == null)
                throw new InvalidOperationException("Tenant not found");

            var existingOrgUnit = await _context.OrgUnits.FirstOrDefaultAsync(
                o => o.Code == request.Code && o.TenantId == request.TenantId);

            if (existingOrgUnit != null)
                throw new InvalidOperationException($"Org unit with code '{request.Code}' already exists in this tenant");

            var orgUnit = new OrgUnit
            {
                Id = Guid.NewGuid(),
                TenantId = request.TenantId,
                Name = request.Name,
                Code = request.Code,
                Type = request.Type,
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.OrgUnits.Add(orgUnit);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Org unit created by {ActorEmail}: {OrgUnitId} - {OrgUnitCode}",
                actorEmail,
                orgUnit.Id,
                orgUnit.Code);

            return orgUnit.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating org unit by {ActorEmail}", actorEmail);
            throw;
        }
    }

    public async Task UpdateOrgUnitAsync(string actorId, string actorEmail, Guid id, UpdateOrgUnitRequest request)
    {
        try
        {
            var orgUnit = await _context.OrgUnits.FirstOrDefaultAsync(o => o.Id == id);
            if (orgUnit == null)
                throw new InvalidOperationException("Org unit not found");

            orgUnit.Name = request.Name;
            orgUnit.Code = request.Code;
            orgUnit.Type = request.Type;
            orgUnit.IsActive = request.IsActive;
            orgUnit.UpdatedAtUtc = DateTime.UtcNow;

            _context.OrgUnits.Update(orgUnit);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Org unit updated by {ActorEmail}: {OrgUnitId}", actorEmail, orgUnit.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating org unit {OrgUnitId} by {ActorEmail}", id, actorEmail);
            throw;
        }
    }

    public async Task DeleteOrgUnitAsync(string actorId, string actorEmail, Guid id)
    {
        try
        {
            var orgUnit = await _context.OrgUnits.FirstOrDefaultAsync(o => o.Id == id);
            if (orgUnit == null)
                throw new InvalidOperationException("Org unit not found");

            _context.OrgUnits.Remove(orgUnit);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Org unit deleted by {ActorEmail}: {OrgUnitId}", actorEmail, orgUnit.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting org unit {OrgUnitId} by {ActorEmail}", id, actorEmail);
            throw;
        }
    }

    public async Task<IEnumerable<AdminUserListItemDto>> GetUsersAsync()
    {
        try
        {
            var users = await _context.Users
                .OrderByDescending(u => u.Id)
                .Select(u => new AdminUserListItemDto
                {
                    Id = u.Id,
                    FullName = $"{u.FirstName} {u.LastName}".Trim(),
                    Email = u.Email ?? "",
                    Role = u.UserType,
                    IsActive = u.IsActive,
                    CreatedAtUtc = DateTime.UtcNow
                })
                .ToListAsync();

            return users;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all users");
            throw;
        }
    }

    public async Task<AdminUserListItemDto?> GetUserByIdAsync(string id)
    {
        try
        {
            var user = await _context.Users
                .Where(u => u.Id == id)
                .Select(u => new AdminUserListItemDto
                {
                    Id = u.Id,
                    FullName = $"{u.FirstName} {u.LastName}".Trim(),
                    Email = u.Email ?? "",
                    Role = u.UserType,
                    IsActive = u.IsActive,
                    CreatedAtUtc = DateTime.UtcNow
                })
                .FirstOrDefaultAsync();

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {UserId}", id);
            throw;
        }
    }

    public async Task<string> CreateUserAsync(string actorId, string actorEmail, CreateUserRequest request)
    {
        try
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
                throw new InvalidOperationException($"User with email '{request.Email}' already exists");

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = request.Email,
                UserName = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                UserType = request.Role,
                TenantId = request.TenantId,
                OrgUnitId = request.OrgUnitId,
                IsActive = true,
                RegistrationStatus = "Active"
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create user: {errors}");
            }

            await _userManager.AddToRoleAsync(user, request.Role);

            _logger.LogInformation("User created by {ActorEmail}: {UserId} - {Email}", actorEmail, user.Id, user.Email);

            return user.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user by {ActorEmail}", actorEmail);
            throw;
        }
    }

    public async Task UpdateUserAsync(string actorId, string actorEmail, string id, UpdateUserRequest request)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                throw new InvalidOperationException("User not found");

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Email = request.Email;
            user.UserName = request.Email;
            user.UserType = request.Role;
            user.TenantId = request.TenantId;
            user.OrgUnitId = request.OrgUnitId;
            user.IsActive = request.IsActive;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to update user: {errors}");
            }

            _logger.LogInformation("User updated by {ActorEmail}: {UserId}", actorEmail, user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId} by {ActorEmail}", id, actorEmail);
            throw;
        }
    }

    public async Task DeleteUserAsync(string actorId, string actorEmail, string id)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                throw new InvalidOperationException("User not found");

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to delete user: {errors}");
            }

            _logger.LogInformation("User deleted by {ActorEmail}: {UserId}", actorEmail, user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId} by {ActorEmail}", id, actorEmail);
            throw;
        }
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
    {
        try
        {
            var totalTenants = await _context.Tenants.CountAsync();
            var totalOrgUnits = await _context.OrgUnits.CountAsync();
            var totalUsers = await _context.Users.CountAsync();
            var totalRemittanceRequests = await _context.RemittanceRequests.CountAsync();
            var totalRemittanceAmount = await _context.RemittanceRequests
                .SumAsync(r => (decimal?)r.Amount) ?? 0;

            var utcToday = DateTime.UtcNow.Date;
            var startDate = utcToday.AddDays(-13);

            var grouped = await _context.RemittanceRequests
                .AsNoTracking()
                .Where(r => r.CreatedAtUtc >= startDate)
                .GroupBy(r => r.CreatedAtUtc.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    TotalAmount = g.Sum(x => x.Amount),
                    TransactionCount = g.Count()
                })
                .ToListAsync();

            var groupedLookup = grouped.ToDictionary(x => x.Date, x => x);

            var dailySeries = Enumerable.Range(0, 14)
                .Select(offset =>
                {
                    var day = startDate.AddDays(offset);
                    groupedLookup.TryGetValue(day, out var bucket);

                    return new DashboardDailyVolumePointDto
                    {
                        Date = day.ToString("yyyy-MM-dd"),
                        TotalAmount = bucket?.TotalAmount ?? 0,
                        TransactionCount = bucket?.TransactionCount ?? 0
                    };
                })
                .ToList();

            return new DashboardSummaryDto
            {
                TotalTenants = totalTenants,
                TotalOrgUnits = totalOrgUnits,
                TotalUsers = totalUsers,
                TotalRemittanceRequests = totalRemittanceRequests,
                TotalRemittanceAmount = totalRemittanceAmount,
                DailyRemittanceVolume = dailySeries
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard summary");
            throw;
        }
    }

    public async Task<IEnumerable<AdminTransactionListItemDto>> GetTransactionsAsync()
    {
        try
        {
            var sourceOrgUnits = _context.OrgUnits.AsNoTracking();
            var destinationOrgUnits = _context.OrgUnits.AsNoTracking();
            var tenants = _context.Tenants.AsNoTracking();

            var rows = await (
                from r in _context.RemittanceRequests.AsNoTracking()
                join t in tenants on r.TenantId equals t.Id into tenantJoin
                from tenant in tenantJoin.DefaultIfEmpty()
                join s in sourceOrgUnits on r.SourceOrgUnitId equals s.Id into sourceJoin
                from source in sourceJoin.DefaultIfEmpty()
                join d in destinationOrgUnits on r.DestinationOrgUnitId equals d.Id into destinationJoin
                from destination in destinationJoin.DefaultIfEmpty()
                orderby r.CreatedAtUtc descending
                select new AdminTransactionListItemDto
                {
                    Id = r.Id,
                    TenantId = r.TenantId,
                    TenantName = tenant != null ? tenant.Name : "Unknown tenant",
                    SourceOrgUnitId = r.SourceOrgUnitId,
                    SourceOrgUnitName = source != null ? source.Name : "Unknown source",
                    DestinationOrgUnitId = r.DestinationOrgUnitId,
                    DestinationOrgUnitName = destination != null ? destination.Name : "Unknown destination",
                    SubmittedByUserId = r.SubmittedByUserId,
                    BeneficiaryName = r.BeneficiaryName,
                    BeneficiaryCountryCode = r.BeneficiaryCountryCode,
                    BeneficiaryCity = r.BeneficiaryCity,
                    Amount = r.Amount,
                    Currency = r.Currency,
                    CurrentQueue = r.CurrentQueue,
                    CurrentStatus = r.CurrentStatus,
                    LastActionByUserId = r.LastActionByUserId,
                    Priority = r.Priority,
                    CreatedAtUtc = r.CreatedAtUtc,
                    UpdatedAtUtc = r.UpdatedAtUtc,
                    Reference = "TXN-" + r.Id.ToString().Replace("-", "").Substring(0, 8).ToUpper()
                })
                .ToListAsync();

            return rows;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transactions");
            throw;
        }
    }

    public async Task<AdminTransactionListItemDto?> GetTransactionByIdAsync(Guid id)
    {
        try
        {
            var sourceOrgUnits = _context.OrgUnits.AsNoTracking();
            var destinationOrgUnits = _context.OrgUnits.AsNoTracking();
            var tenants = _context.Tenants.AsNoTracking();

            var row = await (
                from r in _context.RemittanceRequests.AsNoTracking()
                join t in tenants on r.TenantId equals t.Id into tenantJoin
                from tenant in tenantJoin.DefaultIfEmpty()
                join s in sourceOrgUnits on r.SourceOrgUnitId equals s.Id into sourceJoin
                from source in sourceJoin.DefaultIfEmpty()
                join d in destinationOrgUnits on r.DestinationOrgUnitId equals d.Id into destinationJoin
                from destination in destinationJoin.DefaultIfEmpty()
                where r.Id == id
                select new AdminTransactionListItemDto
                {
                    Id = r.Id,
                    TenantId = r.TenantId,
                    TenantName = tenant != null ? tenant.Name : "Unknown tenant",
                    SourceOrgUnitId = r.SourceOrgUnitId,
                    SourceOrgUnitName = source != null ? source.Name : "Unknown source",
                    DestinationOrgUnitId = r.DestinationOrgUnitId,
                    DestinationOrgUnitName = destination != null ? destination.Name : "Unknown destination",
                    SubmittedByUserId = r.SubmittedByUserId,
                    BeneficiaryName = r.BeneficiaryName,
                    BeneficiaryCountryCode = r.BeneficiaryCountryCode,
                    BeneficiaryCity = r.BeneficiaryCity,
                    Amount = r.Amount,
                    Currency = r.Currency,
                    CurrentQueue = r.CurrentQueue,
                    CurrentStatus = r.CurrentStatus,
                    LastActionByUserId = r.LastActionByUserId,
                    Priority = r.Priority,
                    CreatedAtUtc = r.CreatedAtUtc,
                    UpdatedAtUtc = r.UpdatedAtUtc,
                    Reference = "TXN-" + r.Id.ToString().Replace("-", "").Substring(0, 8).ToUpper()
                })
                .FirstOrDefaultAsync();

            return row;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transaction {TransactionId}", id);
            throw;
        }
    }

public async Task<IEnumerable<AdminKycReviewDto>> GetKycCasesAsync()
{
    try
    {
        var items = await (
            from k in _context.KycDocuments.AsNoTracking()
            join t in _context.Tenants.AsNoTracking() on k.TenantId equals t.Id into tenantJoin
            from t in tenantJoin.DefaultIfEmpty()
            join o in _context.OrgUnits.AsNoTracking() on k.OrgUnitId equals o.Id into orgUnitJoin
            from o in orgUnitJoin.DefaultIfEmpty()
            join u in _context.Users.AsNoTracking() on k.IdentityUserId equals u.Id into userJoin
            from u in userJoin.DefaultIfEmpty()
            orderby k.Id descending
            select new AdminKycReviewDto
            {
                Id = k.Id,
                TenantId = k.TenantId,
                TenantName = t != null ? t.Name : "Unknown tenant",
                OrgUnitId = k.OrgUnitId,
                OrgUnitName = o != null ? o.Name : "Unknown org unit",
                IdentityUserId = k.IdentityUserId,
                ApplicantName = u != null
                    ? (((u.FirstName ?? "") + " " + (u.LastName ?? "")).Trim())
                    : "",
                ApplicantEmail = u != null ? (u.Email ?? "") : "",
                DocumentType = k.DocumentType ?? "",
                FileName = k.FileName ?? "",
                Status = k.ReviewStatus ?? "Pending",
                SubmittedAtUtc = null
            })
            .ToListAsync();

        return items;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving KYC cases");
        throw;
    }
}
}