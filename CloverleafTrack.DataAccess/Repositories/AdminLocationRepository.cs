using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using Dapper;

namespace CloverleafTrack.DataAccess.Repositories;

public class AdminLocationRepository(IDbConnectionFactory connectionFactory) : IAdminLocationRepository
{
    public async Task<List<Location>> GetAllLocationsAsync()
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Locations ORDER BY Name";
        var locations = await connection.QueryAsync<Location>(sql);
        return locations.ToList();
    }

    public async Task<Location?> GetLocationByIdAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Locations WHERE Id = @Id";
        return await connection.QuerySingleOrDefaultAsync<Location>(sql, new { Id = id });
    }

    public async Task<int> CreateLocationAsync(Location location)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO Locations (Name, City, State, ZipCode, Country)
            OUTPUT INSERTED.Id
            VALUES (@Name, @City, @State, @ZipCode, @Country)";
        return await connection.ExecuteScalarAsync<int>(sql, location);
    }

    public async Task<bool> UpdateLocationAsync(Location location)
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

    public async Task<bool> DeleteLocationAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = "DELETE FROM Locations WHERE Id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    public async Task<List<Location>> GetRecentLocationsAsync(int count = 5)
    {
        using var connection = connectionFactory.CreateConnection();

        // Get locations used most recently in meets
        var sql = @"
            SELECT TOP (@Count) l.*
            FROM Locations l
            INNER JOIN (
                SELECT LocationId, MAX(Date) AS LastUsed
                FROM Meets
                GROUP BY LocationId
            ) m ON l.Id = m.LocationId
            ORDER BY m.LastUsed DESC";

        var locations = await connection.QueryAsync<Location>(sql, new { Count = count });
        return locations.ToList();
    }
}