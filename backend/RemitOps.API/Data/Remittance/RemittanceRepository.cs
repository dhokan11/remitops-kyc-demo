using Microsoft.EntityFrameworkCore;
using RemitOps.API.Data;
using RemitOps.API.Entities;
using RemitOps.API.Models.Filtering;
using RemitOps.API.Models.Remittance;

namespace RemitOps.API.Data.Remittance;

public class RemittanceRepository : IRemittanceRepository
{
    private readonly ApplicationDbContext _context;

    public RemittanceRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all remittances with optional tenant filter
    /// </summary>
    public async Task<IEnumerable<RemittanceListItemDto>> GetRemittancesAsync(Guid? tenantId = null)
    {
        var query = _context.RemittanceRequests
            .Include(r => r.SourceOrgUnit)
            .Include(r => r.DestinationOrgUnit)
            .AsQueryable();

        if (tenantId.HasValue)
            query = query.Where(r => r.TenantId == tenantId.Value);

        return await query
            .Select(r => new RemittanceListItemDto
            {
                Id = r.Id,
                BeneficiaryName = r.BeneficiaryName ?? "",
                Amount = r.Amount,
                SourceOrgUnitName = r.SourceOrgUnit.Name,
                DestinationOrgUnitName = r.DestinationOrgUnit.Name,
                CurrentStatus = r.CurrentStatus,
                CurrentQueue = r.CurrentQueue,
                CreatedAtUtc = r.CreatedAtUtc
            })
            .OrderByDescending(r => r.CreatedAtUtc)
            .ToListAsync();
    }

    /// <summary>
    /// Get detailed remittance by ID with full audit trail
    /// </summary>
    public async Task<RemittanceDetailDto?> GetRemittanceByIdAsync(Guid id)
    {
        var remittance = await _context.RemittanceRequests
            .Include(r => r.SourceOrgUnit)
            .Include(r => r.DestinationOrgUnit)
            .Include(r => r.SubmittedByUser)
            .Include(r => r.AuditTrail)
                .ThenInclude(a => a.PerformedByUser)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (remittance == null)
            return null;

        return new RemittanceDetailDto
        {
            Id = remittance.Id,
            TenantId = remittance.TenantId,
            SourceOrgUnitId = remittance.SourceOrgUnitId,
            DestinationOrgUnitId = remittance.DestinationOrgUnitId,
            SourceOrgUnitName = remittance.SourceOrgUnit.Name,
            DestinationOrgUnitName = remittance.DestinationOrgUnit.Name,
            SubmittedByEmail = remittance.SubmittedByUser.Email ?? "",
            BeneficiaryName = remittance.BeneficiaryName,
            Amount = remittance.Amount,
            CurrentQueue = remittance.CurrentQueue,
            CurrentStatus = remittance.CurrentStatus,
            CreatedAtUtc = remittance.CreatedAtUtc,
            UpdatedAtUtc = remittance.UpdatedAtUtc,
            AuditTrail = remittance.AuditTrail
                .OrderByDescending(a => a.PerformedAtUtc)
                .Select(a => new RemittanceAuditDto
                {
                    Id = a.Id,
                    Action = a.Action,
                    PerformedByEmail = a.PerformedByUser.Email ?? "",
                    Notes = a.Notes,
                    PerformedAtUtc = a.PerformedAtUtc
                })
                .ToList()
        };
    }

    /// <summary>
    /// Create a new remittance request with automatic audit entry
    /// </summary>
    public async Task<Guid> CreateRemittanceAsync(Guid tenantId, string userId, CreateRemittanceRequest request)
    {
        var remittance = new RemittanceRequest
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SourceOrgUnitId = request.SourceOrgUnitId,
            DestinationOrgUnitId = request.DestinationOrgUnitId,
            SubmittedByUserId = userId,
            BeneficiaryName = request.BeneficiaryName,
            Amount = request.Amount,
            Currency = "USD",
            CurrentQueue = "Pending",
            CurrentStatus = "Submitted",
            Priority = "Normal",
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.RemittanceRequests.Add(remittance);

        var audit = new RemittanceAudit
        {
            Id = Guid.NewGuid(),
            RemittanceRequestId = remittance.Id,
            PerformedByUserId = userId,
            Action = "CREATED",
            Notes = "Remittance request created",
            PerformedAtUtc = DateTime.UtcNow
        };

        _context.RemittanceAudits.Add(audit);
        await _context.SaveChangesAsync();

        return remittance.Id;
    }

    /// <summary>
    /// Update remittance status with audit trail entry
    /// </summary>
    public async Task UpdateRemittanceStatusAsync(Guid id, string userId, UpdateRemittanceStatusRequest request)
    {
        var remittance = await _context.RemittanceRequests.FindAsync(id);
        if (remittance == null)
            throw new InvalidOperationException($"Remittance with ID {id} not found.");

        remittance.CurrentStatus = request.NewStatus;
        remittance.LastActionByUserId = userId;
        remittance.UpdatedAtUtc = DateTime.UtcNow;

        var audit = new RemittanceAudit
        {
            Id = Guid.NewGuid(),
            RemittanceRequestId = remittance.Id,
            PerformedByUserId = userId,
            Action = $"STATUS_CHANGED_TO_{request.NewStatus.ToUpper()}",
            Notes = request.Notes,
            PerformedAtUtc = DateTime.UtcNow
        };

        _context.RemittanceAudits.Add(audit);
        _context.RemittanceRequests.Update(remittance);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Get remittance audit trail ordered by date descending
    /// </summary>
    public async Task<IEnumerable<RemittanceAuditDto>> GetRemittanceAuditTrailAsync(Guid remittanceId)
    {
        return await _context.RemittanceAudits
            .Where(a => a.RemittanceRequestId == remittanceId)
            .Include(a => a.PerformedByUser)
            .OrderByDescending(a => a.PerformedAtUtc)
            .Select(a => new RemittanceAuditDto
            {
                Id = a.Id,
                Action = a.Action,
                PerformedByEmail = a.PerformedByUser.Email ?? "",
                Notes = a.Notes,
                PerformedAtUtc = a.PerformedAtUtc
            })
            .ToListAsync();
    }

    /// <summary>
    /// Advanced filtering with pagination, sorting, and multiple criteria
    /// Supports: tenant, org units, status, queue, priority, currency, geography, amount, date range, tags
    /// </summary>
    public async Task<PaginatedResult<RemittanceListItemDto>> FilterRemittancesAsync(RemittanceFilterRequest filter)
    {
        var query = _context.RemittanceRequests
            .Include(r => r.SourceOrgUnit)
            .Include(r => r.DestinationOrgUnit)
            .Include(r => r.Tags)
                .ThenInclude(t => t.Tag)
            .AsQueryable();

        // ===== APPLY FILTERS =====

        // Tenant filter
        if (filter.TenantId.HasValue)
            query = query.Where(r => r.TenantId == filter.TenantId.Value);

        // Organization Unit filters
        if (filter.SourceOrgUnitId.HasValue)
            query = query.Where(r => r.SourceOrgUnitId == filter.SourceOrgUnitId.Value);

        if (filter.DestinationOrgUnitId.HasValue)
            query = query.Where(r => r.DestinationOrgUnitId == filter.DestinationOrgUnitId.Value);

        // Status, Queue, Priority filters
        if (!string.IsNullOrEmpty(filter.Status))
            query = query.Where(r => r.CurrentStatus == filter.Status);

        if (!string.IsNullOrEmpty(filter.Queue))
            query = query.Where(r => r.CurrentQueue == filter.Queue);

        if (!string.IsNullOrEmpty(filter.Priority))
            query = query.Where(r => r.Priority == filter.Priority);

        // Currency filter
        if (!string.IsNullOrEmpty(filter.Currency))
            query = query.Where(r => r.Currency == filter.Currency);

        // Geographic filters - Beneficiary
        if (!string.IsNullOrEmpty(filter.BeneficiaryCountryCode))
            query = query.Where(r => r.BeneficiaryCountryCode == filter.BeneficiaryCountryCode);

        // Geographic filters - Source OrgUnit
        if (!string.IsNullOrEmpty(filter.SourceCountryCode))
            query = query.Where(r => r.SourceOrgUnit.CountryCode == filter.SourceCountryCode);

        if (!string.IsNullOrEmpty(filter.SourceCity))
            query = query.Where(r => r.SourceOrgUnit.City == filter.SourceCity);

        // Geographic filters - Destination OrgUnit
        if (!string.IsNullOrEmpty(filter.DestinationCountryCode))
            query = query.Where(r => r.DestinationOrgUnit.CountryCode == filter.DestinationCountryCode);

        if (!string.IsNullOrEmpty(filter.DestinationCity))
            query = query.Where(r => r.DestinationOrgUnit.City == filter.DestinationCity);

        // Amount range filter
        if (filter.MinAmount.HasValue)
            query = query.Where(r => r.Amount >= filter.MinAmount.Value);

        if (filter.MaxAmount.HasValue)
            query = query.Where(r => r.Amount <= filter.MaxAmount.Value);

        // Date range filter
        if (filter.StartDate.HasValue)
            query = query.Where(r => r.CreatedAtUtc >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(r => r.CreatedAtUtc <= filter.EndDate.Value);

        // Tag filter (multi-select)
        if (filter.Tags != null && filter.Tags.Count > 0)
            query = query.Where(r => r.Tags.Any(rt => filter.Tags.Contains(rt.Tag.Name)));

        // ===== GET TOTAL COUNT =====
        var totalCount = await query.CountAsync();

        // ===== APPLY SORTING =====
        query = ApplySorting(query, filter.SortBy, filter.SortDescending);

        // ===== APPLY PAGINATION =====
        var items = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(r => new RemittanceListItemDto
            {
                Id = r.Id,
                BeneficiaryName = r.BeneficiaryName ?? "",
                Amount = r.Amount,
                SourceOrgUnitName = r.SourceOrgUnit.Name,
                DestinationOrgUnitName = r.DestinationOrgUnit.Name,
                CurrentStatus = r.CurrentStatus,
                CurrentQueue = r.CurrentQueue,
                CreatedAtUtc = r.CreatedAtUtc
            })
            .ToListAsync();

        return new PaginatedResult<RemittanceListItemDto>
        {
            Items = items,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize,
            TotalCount = totalCount
        };
    }

    /// <summary>
    /// Get remittance corridors (source country -> destination country) with statistics
    /// Used for dashboard corridor visualization
    /// </summary>
    public async Task<List<RemittanceCorridorDto>> GetRemittanceCorridorsAsync(Guid tenantId)
    {
        var corridors = await _context.RemittanceRequests
            .Where(r => r.TenantId == tenantId)
            .Include(r => r.SourceOrgUnit)
                .ThenInclude(ou => ou.GeoLocation)
            .Include(r => r.DestinationOrgUnit)
                .ThenInclude(ou => ou.GeoLocation)
            .GroupBy(r => new
            {
                SourceCountry = r.SourceOrgUnit.CountryCode,
                DestinationCountry = r.DestinationOrgUnit.CountryCode,
                SourceCountryName = r.SourceOrgUnit.GeoLocation != null ? r.SourceOrgUnit.GeoLocation.CountryName : "Unknown",
                DestinationCountryName = r.DestinationOrgUnit.GeoLocation != null ? r.DestinationOrgUnit.GeoLocation.CountryName : "Unknown"
            })
            .Select(g => new RemittanceCorridorDto
            {
                SourceCountryCode = g.Key.SourceCountry ?? "",
                SourceCountryName = g.Key.SourceCountryName ?? "Unknown",
                DestinationCountryCode = g.Key.DestinationCountry ?? "",
                DestinationCountryName = g.Key.DestinationCountryName ?? "Unknown",
                RemittanceCount = g.Count(),
                TotalAmount = g.Sum(r => r.Amount),
                LatestRemittanceDate = g.Max(r => r.CreatedAtUtc)
            })
            .OrderByDescending(c => c.TotalAmount)
            .ToListAsync();

        return corridors;
    }

    /// <summary>
    /// Get remittance summary statistics for dashboard
    /// Includes: totals, averages, status breakdown, queue breakdown, top countries
    /// </summary>
    public async Task<RemittanceSummaryStatsDto> GetRemittanceSummaryStatsAsync(Guid tenantId)
    {
        var remittances = await _context.RemittanceRequests
            .Where(r => r.TenantId == tenantId)
            .ToListAsync();

        var statusBreakdown = remittances
            .GroupBy(r => r.CurrentStatus)
            .Select(g => new StatusBreakdownDto
            {
                Status = g.Key,
                Count = g.Count(),
                TotalAmount = g.Sum(r => r.Amount)
            })
            .ToList();

        var queueBreakdown = remittances
            .GroupBy(r => r.CurrentQueue)
            .Select(g => new QueueBreakdownDto
            {
                Queue = g.Key,
                Count = g.Count(),
                TotalAmount = g.Sum(r => r.Amount)
            })
            .ToList();

        var countryBreakdown = remittances
            .GroupBy(r => r.BeneficiaryCountryCode)
            .Select(g => new CountryBreakdownDto
            {
                CountryCode = g.Key ?? "UNKNOWN",
                RemittanceCount = g.Count(),
                TotalAmount = g.Sum(r => r.Amount)
            })
            .OrderByDescending(c => c.TotalAmount)
            .Take(10)
            .ToList();

        return new RemittanceSummaryStatsDto
        {
            TotalRemittances = remittances.Count,
            TotalAmount = remittances.Sum(r => r.Amount),
            AverageAmount = remittances.Count > 0 ? remittances.Average(r => r.Amount) : 0,
            PendingCount = remittances.Count(r => r.CurrentStatus == "Pending"),
            CompletedCount = remittances.Count(r => r.CurrentStatus == "Completed"),
            RejectedCount = remittances.Count(r => r.CurrentStatus == "Rejected"),
            StatusBreakdown = statusBreakdown,
            QueueBreakdown = queueBreakdown,
            TopCountries = countryBreakdown
        };
    }

    /// <summary>
    /// Apply dynamic sorting based on sort field and direction
    /// Supports: amount, status, queue, priority, source, destination, beneficiary, updated, created
    /// </summary>
    private IQueryable<RemittanceRequest> ApplySorting(
        IQueryable<RemittanceRequest> query,
        string? sortBy,
        bool descending)
    {
        return (sortBy?.ToLower()) switch
        {
            "amount" => descending
                ? query.OrderByDescending(r => r.Amount)
                : query.OrderBy(r => r.Amount),

            "status" => descending
                ? query.OrderByDescending(r => r.CurrentStatus)
                : query.OrderBy(r => r.CurrentStatus),

            "queue" => descending
                ? query.OrderByDescending(r => r.CurrentQueue)
                : query.OrderBy(r => r.CurrentQueue),

            "priority" => descending
                ? query.OrderByDescending(r => r.Priority)
                : query.OrderBy(r => r.Priority),

            "source" => descending
                ? query.OrderByDescending(r => r.SourceOrgUnit.Name)
                : query.OrderBy(r => r.SourceOrgUnit.Name),

            "destination" => descending
                ? query.OrderByDescending(r => r.DestinationOrgUnit.Name)
                : query.OrderBy(r => r.DestinationOrgUnit.Name),

            "beneficiary" => descending
                ? query.OrderByDescending(r => r.BeneficiaryName)
                : query.OrderBy(r => r.BeneficiaryName),

            "updated" => descending
                ? query.OrderByDescending(r => r.UpdatedAtUtc)
                : query.OrderBy(r => r.UpdatedAtUtc),

            _ => descending
                ? query.OrderByDescending(r => r.CreatedAtUtc)
                : query.OrderBy(r => r.CreatedAtUtc)
        };
    }
}

// ===== ADDITIONAL DTO CLASSES FOR REMITTANCE STATISTICS =====

/// <summary>
/// DTO for remittance corridor visualization (country-pair analysis)
/// </summary>
public class RemittanceCorridorDto
{
    public string SourceCountryCode { get; set; } = "";
    public string SourceCountryName { get; set; } = "";
    public string DestinationCountryCode { get; set; } = "";
    public string DestinationCountryName { get; set; } = "";
    public int RemittanceCount { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime LatestRemittanceDate { get; set; }
}

/// <summary>
/// DTO for comprehensive remittance statistics and dashboard metrics
/// </summary>
public class RemittanceSummaryStatsDto
{
    public int TotalRemittances { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AverageAmount { get; set; }
    public int PendingCount { get; set; }
    public int CompletedCount { get; set; }
    public int RejectedCount { get; set; }
    public List<StatusBreakdownDto> StatusBreakdown { get; set; } = new();
    public List<QueueBreakdownDto> QueueBreakdown { get; set; } = new();
    public List<CountryBreakdownDto> TopCountries { get; set; } = new();
}

/// <summary>
/// DTO for status-based remittance statistics
/// </summary>
public class StatusBreakdownDto
{
    public string Status { get; set; } = "";
    public int Count { get; set; }
    public decimal TotalAmount { get; set; }
}

/// <summary>
/// DTO for queue-based remittance statistics
/// </summary>
public class QueueBreakdownDto
{
    public string Queue { get; set; } = "";
    public int Count { get; set; }
    public decimal TotalAmount { get; set; }
}

/// <summary>
/// DTO for country-based remittance distribution
/// </summary>
public class CountryBreakdownDto
{
    public string CountryCode { get; set; } = "";
    public int RemittanceCount { get; set; }
    public decimal TotalAmount { get; set; }
}