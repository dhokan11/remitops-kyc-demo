namespace RemitOps.API.Models.Admin;

public class DashboardDailyVolumePointDto
{
    public string Date { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public int TransactionCount { get; set; }
}

public class DashboardSummaryDto
{
    public int TotalTenants { get; set; }
    public int TotalOrgUnits { get; set; }
    public int TotalUsers { get; set; }
    public int TotalRemittanceRequests { get; set; }
    public decimal TotalRemittanceAmount { get; set; }
    public List<DashboardDailyVolumePointDto> DailyRemittanceVolume { get; set; } = new();
}