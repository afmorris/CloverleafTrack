using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using Dapper;

namespace CloverleafTrack.DataAccess.Repositories;

public class AdminSeasonRepository(IDbConnectionFactory connectionFactory) : IAdminSeasonRepository
{
    public async Task<List<Season>> GetAllAsync()
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Seasons ORDER BY StartDate DESC";
        var seasons = await connection.QueryAsync<Season>(sql);
        return seasons.ToList();
    }

    public async Task<Season?> GetByIdAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Seasons WHERE Id = @Id";
        return await connection.QuerySingleOrDefaultAsync<Season>(sql, new { Id = id });
    }

    public async Task<Season?> GetCurrentSeasonAsync()
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = "SELECT TOP 1 * FROM Seasons WHERE IsCurrentSeason = 1";
        return await connection.QuerySingleOrDefaultAsync<Season>(sql);
    }

    public async Task<int> CreateAsync(Season season)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO Seasons (Name, StartDate, EndDate, IsCurrentSeason, Notes, Status)
            OUTPUT INSERTED.Id
            VALUES (@Name, @StartDate, @EndDate, @IsCurrentSeason, @Notes, @Status)";
        return await connection.ExecuteScalarAsync<int>(sql, season);
    }

    public async Task<bool> UpdateAsync(Season season)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE Seasons
            SET Name = @Name,
                StartDate = @StartDate,
                EndDate = @EndDate,
                IsCurrentSeason = @IsCurrentSeason,
                Notes = @Notes,
                Status = @Status
            WHERE Id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, season);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = "DELETE FROM Seasons WHERE Id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    public async Task<Season?> GetSeasonForDateAsync(DateTime date)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            SELECT TOP 1 * FROM Seasons
            WHERE @Date BETWEEN StartDate AND EndDate
            ORDER BY StartDate DESC";
        return await connection.QuerySingleOrDefaultAsync<Season>(sql, new { Date = date });
    }
}
