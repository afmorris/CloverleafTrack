using System.Data;
using CloverleafTrack.DataAccess.Interfaces;
using Microsoft.Data.SqlClient;

namespace CloverleafTrack.DataAccess;

public class SqlConnectionFactory(string connectionString) : IDbConnectionFactory
{
    public IDbConnection CreateConnection()
    {
        return new SqlConnection(connectionString);
    }
}