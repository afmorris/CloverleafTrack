using System.Data;

namespace CloverleafTrack.DataAccess.Interfaces;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}