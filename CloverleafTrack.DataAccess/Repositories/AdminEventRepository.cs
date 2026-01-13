using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using CloverleafTrack.Models.Enums;
using Dapper;
using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.DataAccess.Repositories;

public class AdminEventRepository(IDbConnectionFactory connectionFactory) : IAdminEventRepository
{
    public async Task<List<Event>> GetAllEventsAsync()
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            SELECT * FROM Events 
            ORDER BY Environment, Gender, EventCategorySortOrder, SortOrder";
        var events = await connection.QueryAsync<Event>(sql);
        return events.ToList();
    }

    public async Task<List<Event>> GetEventsByEnvironmentAndGenderAsync(Environment environment, Gender? gender)
    {
        using var connection = connectionFactory.CreateConnection();

        const string sql = @"
            SELECT * FROM Events 
            WHERE Environment = @Environment
                AND (@Gender IS NULL OR Gender = @Gender OR Gender IS NULL)
            ORDER BY EventCategorySortOrder, SortOrder";

        var events = await connection.QueryAsync<Event>(sql, new { Environment = environment, Gender = gender });
        return events.ToList();
    }

    public async Task<Event?> GetEventByIdAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Events WHERE Id = @Id";
        return await connection.QuerySingleOrDefaultAsync<Event>(sql, new { Id = id });
    }

    public async Task<int> CreateEventAsync(Event eventItem)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO Events (Name, EventKey, EventType, Gender, Environment, AthleteCount, SortOrder, EventCategory, EventCategorySortOrder)
            OUTPUT INSERTED.Id
            VALUES (@Name, @EventKey, @EventType, @Gender, @Environment, @AthleteCount, @SortOrder, @EventCategory, @EventCategorySortOrder)";
        return await connection.ExecuteScalarAsync<int>(sql, eventItem);
    }

    public async Task<bool> UpdateEventAsync(Event eventItem)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE Events 
            SET Name = @Name,
                EventKey = @EventKey,
                EventType = @EventType,
                Gender = @Gender,
                Environment = @Environment,
                AthleteCount = @AthleteCount,
                SortOrder = @SortOrder,
                EventCategory = @EventCategory,
                EventCategorySortOrder = @EventCategorySortOrder
            WHERE Id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, eventItem);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteEventAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = "DELETE FROM Events WHERE Id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }
}