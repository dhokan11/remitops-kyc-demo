namespace RemitOps.API.Models
{
    public class DatabaseSeedingOptions
    {
        public bool EnsureDatabase { get; set; } = true;
        public bool RunMigrations { get; set; } = true;
        public bool SeedIdentity { get; set; } = true;
        public bool SeedSqlObjects { get; set; } = true;
        public bool SeedReferenceData { get; set; } = true;
        public bool SeedDemoData { get; set; } = true;
    }
}