using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace RemitOps.API.Services
{
    public class DatabaseBootstrapper
    {
        private readonly IConfiguration _configuration;

        public DatabaseBootstrapper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task EnsureDatabaseExistsAsync()
        {
            var masterConnection = _configuration.GetConnectionString("MasterConnection")
                ?? throw new InvalidOperationException("MasterConnection missing.");
            var appConnection = _configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection missing.");

            var builder = new SqlConnectionStringBuilder(appConnection);
            var dbName = builder.InitialCatalog;

            using var db = new SqlConnection(masterConnection);
            await db.ExecuteAsync($@"
IF DB_ID('{dbName}') IS NULL
BEGIN
    CREATE DATABASE [{dbName}];
END;
");
        }
    }
}