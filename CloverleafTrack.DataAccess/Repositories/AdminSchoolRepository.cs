using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using Dapper;

namespace CloverleafTrack.DataAccess.Repositories;

public class AdminSchoolRepository(IDbConnectionFactory connectionFactory) : IAdminSchoolRepository
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

    public async Task<int> CreateAsync(School school)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO Schools (Name, ShortName)
            OUTPUT INSERTED.Id
            VALUES (@Name, @ShortName)";
        return await connection.ExecuteScalarAsync<int>(sql, school);
    }

    public async Task<bool> UpdateAsync(School school)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE Schools
            SET Name = @Name, ShortName = @ShortName, DateUpdated = GETUTCDATE()
            WHERE Id = @Id AND Deleted = 0";
        var rows = await connection.ExecuteAsync(sql, school);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = "UPDATE Schools SET Deleted = 1, DateDeleted = GETUTCDATE() WHERE Id = @Id";
        var rows = await connection.ExecuteAsync(sql, new { Id = id });
        return rows > 0;
    }
}
