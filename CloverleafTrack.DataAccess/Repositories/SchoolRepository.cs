using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using Dapper;

namespace CloverleafTrack.DataAccess.Repositories;

public class SchoolRepository(IDbConnectionFactory connectionFactory) : ISchoolRepository
{
    public async Task<List<School>> GetAllAsync()
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Schools WHERE Deleted = 0 ORDER BY Name";
        var results = await connection.QueryAsync<School>(sql);
        return results.ToList();
    }

    public async Task<School?> GetByIdAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Schools WHERE Id = @Id AND Deleted = 0";
        return await connection.QuerySingleOrDefaultAsync<School>(sql, new { Id = id });
    }
}
