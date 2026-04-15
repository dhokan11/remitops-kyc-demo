using System.Data;
using Microsoft.Data.SqlClient;

namespace RemitOps.API.Data
{
    public class SqlConnectionFactory : IDbConnectionFactory
    {
        private readonly IConfiguration _configuration;

        public SqlConnectionFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IDbConnection CreateConnection()
        {
            var connectionString =
                _configuration.GetConnectionString("AppConnection") ??
                _configuration.GetConnectionString("DefaultConnection") ??
                _configuration.GetConnectionString("IdentityConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("No SQL connection string found.");

            return new SqlConnection(connectionString);
        }
    }
}