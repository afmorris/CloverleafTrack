using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using Dapper;

namespace CloverleafTrack.DataAccess.Repositories;

public class AdminLocationRepository(IDbConnectionFactory connectionFactory) : IAdminLocationRepository
{
    public async Task<List<Location>> GetAllAsync()
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Locations ORDER BY Name";
        var locations = await connection.QueryAsync<Location>(sql);
        return locations.ToList();
    }

    public async Task<Location?> GetByIdAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Locations WHERE Id = @Id";
        return await connection.QuerySingleOrDefaultAsync<Location>(sql, new { Id = id });
    }

    public async Task<int> CreateAsync(Location location)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO Locations (Name, City, State, ZipCode, Country)
            OUTPUT INSERTED.Id
            VALUES (@Name, @City, @State, @ZipCode, @Country)";
        return await connection.ExecuteScalarAsync<int>(sql, location);
    }

    public async Task<bool> UpdateAsync(Location location)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE Locations
            SET Name = @Name,
                City = @City,
                State = @State,
                ZipCode = @ZipCode,
                Country = @Country
            WHERE Id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, location);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = "DELETE FROM Locations WHERE Id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    public async Task<List<Location>> GetRecentlyUsedAsync(int count = 5)
    {
        using var connection = connectionFactory.CreateConnection();
        var sql = $@"
            SELECT TOP {count} l.*
            FROM Locations l
            WHERE l.Id IN (
                SELECT TOP {count} LocationId
                FROM (
                    SELECT DISTINCT LocationId, MAX(Date) as MaxDate
                    FROM Meets
                    GROUP BY LocationId
                ) AS RecentLocations
                ORDER BY MaxDate DESC
            )";
        var locations = await connection.QueryAsync<Location>(sql);
        return locations.ToList();
    }
}
