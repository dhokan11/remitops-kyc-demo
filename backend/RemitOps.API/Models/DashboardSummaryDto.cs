namespace RemitOps.API.Models;

public class DashboardSummaryDto
{
    public int TotalTx { get; set; }
    public int Completed { get; set; }
    public int Failed { get; set; }
    public decimal TotalVolume { get; set; }
}