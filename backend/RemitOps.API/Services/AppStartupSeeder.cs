using Microsoft.Extensions.Options;
using RemitOps.API.Models;

namespace RemitOps.API.Services
{
    public class AppStartupSeeder
    {
        private readonly DatabaseBootstrapper _bootstrapper;
        private readonly IdentityMigrationRunner _migrations;
        private readonly IdentitySeedService _identitySeed;
        private readonly SqlSchemaSeeder _schemaSeeder;
        private readonly DemoDataSeeder _demoSeeder;
        private readonly DatabaseSeedingOptions _options;

        public AppStartupSeeder(
            DatabaseBootstrapper bootstrapper,
            IdentityMigrationRunner migrations,
            IdentitySeedService identitySeed,
            SqlSchemaSeeder schemaSeeder,
            DemoDataSeeder demoSeeder,
            IOptions<DatabaseSeedingOptions> options)
        {
            _bootstrapper = bootstrapper;
            _migrations = migrations;
            _identitySeed = identitySeed;
            _schemaSeeder = schemaSeeder;
            _demoSeeder = demoSeeder;
            _options = options.Value;
        }

        public async Task RunAsync()
        {
            if (_options.EnsureDatabase)
                await _bootstrapper.EnsureDatabaseExistsAsync();

            if (_options.RunMigrations)
                await _migrations.RunAsync();

            if (_options.SeedIdentity)
                await _identitySeed.SeedAsync();

            if (_options.SeedSqlObjects)
                await _schemaSeeder.SeedAsync();

            if (_options.SeedReferenceData || _options.SeedDemoData)
                await _demoSeeder.SeedAsync();
        }
    }
}