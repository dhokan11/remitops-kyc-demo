namespace RemitOps.API.Models
{
    public class AuditSeedingOptions
    {
        public bool EnsureObjects { get; set; } = true;
        public bool SeedDemoData { get; set; } = true;
    }
}