using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using CloverleafTrack.Models.Enums;
using Dapper;
using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.DataAccess.Repositories;

public class AdminEventRepository(IDbConnectionFactory connectionFactory) : IAdminEventRepository
{
    public async Task<List<Event>> GetAllAsync()
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            SELECT * FROM Events
            ORDER BY Gender, Environment DESC, EventCategorySortOrder, SortOrder";
        var events = await connection.QueryAsync<Event>(sql);
        return events.ToList();
    }

    public async Task<Event?> GetByIdAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Events WHERE Id = @Id";
        return await connection.QuerySingleOrDefaultAsync<Event>(sql, new { Id = id });
    }

    public async Task<int> CreateAsync(Event evt)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO Events (Name, EventKey, EventType, Gender, Environment, AthleteCount, SortOrder, EventCategory, EventCategorySortOrder)
            OUTPUT INSERTED.Id
            VALUES (@Name, @EventKey, @EventType, @Gender, @Environment, @AthleteCount, @SortOrder, @EventCategory, @EventCategorySortOrder)";
        return await connection.ExecuteScalarAsync<int>(sql, evt);
    }

    public async Task<bool> UpdateAsync(Event evt)
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
        var rowsAffected = await connection.ExecuteAsync(sql, evt);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = "DELETE FROM Events WHERE Id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    public async Task<List<Event>> GetByGenderAndEnvironmentAsync(Gender? gender, Environment environment)
    {
        using var connection = connectionFactory.CreateConnection();
        
        var sql = @"
            SELECT * FROM Events
            WHERE Environment = @Environment";
        
        var parameters = new DynamicParameters();
        parameters.Add("Environment", environment);
        
        if (gender.HasValue)
        {
            sql += " AND (Gender = @Gender OR Gender IS NULL)";
            parameters.Add("Gender", gender.Value);
        }
        
        sql += " ORDER BY EventCategorySortOrder, SortOrder";
        
        var events = await connection.QueryAsync<Event>(sql, parameters);
        return events.ToList();
    }
}
