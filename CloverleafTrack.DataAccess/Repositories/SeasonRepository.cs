using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using Dapper;

namespace CloverleafTrack.DataAccess.Repositories;

public class SeasonRepository(IDbConnectionFactory connectionFactory) : ISeasonRepository
{
    public async Task<List<Season>> GetAllAsync()
    {
        using var connection = connectionFactory.CreateConnection();
        var sql = "SELECT * FROM Seasons";
        var result = await connection.QueryAsync<Season>(sql);
        return result.ToList();
    }

    public async Task<Season?> GetByIdAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        var sql = "SELECT * FROM Seasons WHERE Id = @Id";
        return await connection.QueryFirstOrDefaultAsync<Season>(sql, new { Id = id });
    }

    public async Task<int> CreateAsync(Season season)
    {
        using var connection = connectionFactory.CreateConnection();
        var sql = @"
            INSERT INTO Seasons (Name, StartDate, EndDate)
            VALUES (@Name, @StartDate, @EndDate);
            SELECT CAST(SCOPE_IDENTITY() as int)";
        return await connection.ExecuteScalarAsync<int>(sql, season);
    }

    public async Task<bool> UpdateAsync(Season season)
    {
        using var connection = connectionFactory.CreateConnection();
        var sql = @"
            UPDATE Seasons
            SET Name = @Name,
                StartDate = @StartDate,
                EndDate = @EndDate
            WHERE Id = @Id";
        var affectedRows = await connection.ExecuteAsync(sql, season);
        return affectedRows > 0;
    }

    public async Task<bool> DeleteAsync(Season season)
    {
        using var connection = connectionFactory.CreateConnection();
        var sql = "DELETE FROM Seasons WHERE Id = @Id";
        var affectedRows = await connection.ExecuteAsync(sql, new { season.Id });
        return affectedRows > 0;
    }
}