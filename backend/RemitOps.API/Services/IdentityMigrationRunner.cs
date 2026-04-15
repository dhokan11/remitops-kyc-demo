using RemitOps.API.Data;
using Microsoft.EntityFrameworkCore;

namespace RemitOps.API.Services
{
    public class IdentityMigrationRunner
    {
        private readonly ApplicationDbContext _db;

        public IdentityMigrationRunner(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task RunAsync()
        {
            await _db.Database.MigrateAsync();
        }
    }
}