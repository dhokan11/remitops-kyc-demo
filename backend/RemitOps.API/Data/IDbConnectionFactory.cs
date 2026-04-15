using System.Data;

namespace RemitOps.API.Data
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}