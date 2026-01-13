using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using CloverleafTrack.Models.Enums;
using Dapper;
using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.DataAccess.Repositories;

public class AdminMeetRepository(IDbConnectionFactory connectionFactory) : IAdminMeetRepository
{
    public async Task<List<Meet>> GetAllAsync()
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            SELECT 
                m.*,
                l.*,
                s.*
            FROM Meets m
            INNER JOIN Locations l ON m.LocationId = l.Id
            INNER JOIN Seasons s ON s.Id = m.SeasonId
            ORDER BY m.Date DESC";

        var meets = await connection.QueryAsync<Meet, Location, Season, Meet>(
            sql,
            (meet, location, season) =>
            {
                meet.Location = location;
                meet.Season = season;
                return meet;
            },
            splitOn: "Id,Id");

        return meets.ToList();
    }

    public async Task<List<Meet>> GetFilteredAsync(string? searchName, int? seasonId, Environment? environment, MeetEntryStatus? entryStatus)
    {
        using var connection = connectionFactory.CreateConnection();

        var sql = @"
            SELECT 
                m.*,
                l.*,
                s.*
            FROM Meets m
            INNER JOIN Locations l ON m.LocationId = l.Id
            INNER JOIN Seasons s ON s.Id = m.SeasonId
            WHERE 1=1";

        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(searchName))
        {
            sql += " AND m.Name LIKE @SearchName";
            parameters.Add("SearchName", $"%{searchName}%");
        }

        if (seasonId.HasValue)
        {
            sql += " AND m.SeasonId = @SeasonId";
            parameters.Add("SeasonId", seasonId.Value);
        }

        if (environment.HasValue)
        {
            sql += " AND m.Environment = @Environment";
            parameters.Add("Environment", environment.Value);
        }

        if (entryStatus.HasValue)
        {
            sql += " AND m.EntryStatus = @EntryStatus";
            parameters.Add("EntryStatus", entryStatus.Value);
        }

        sql += " ORDER BY m.Date DESC";

        var meets = await connection.QueryAsync<Meet, Location, Season, Meet>(
            sql,
            (meet, location, season) =>
            {
                meet.Location = location;
                meet.Season = season;
                return meet;
            },
            parameters,
            splitOn: "Id,Id");

        return meets.ToList();
    }

    public async Task<Meet?> GetByIdAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Meets WHERE Id = @Id";
        return await connection.QuerySingleOrDefaultAsync<Meet>(sql, new { Id = id });
    }

    public async Task<Meet?> GetByIdWithDetailsAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            SELECT 
                m.*,
                l.*,
                s.*
            FROM Meets m
            INNER JOIN Locations l ON m.LocationId = l.Id
            INNER JOIN Seasons s ON s.Id = m.SeasonId
            WHERE m.Id = @Id";

        var meets = await connection.QueryAsync<Meet, Location, Season, Meet>(
            sql,
            (meet, location, season) =>
            {
                meet.Location = location;
                meet.Season = season;
                return meet;
            },
            new { Id = id },
            splitOn: "Id,Id");

        return meets.FirstOrDefault();
    }

    public async Task<int> CreateAsync(Meet meet)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO Meets (Name, Date, LocationId, Environment, HandTimed, SeasonId, EntryStatus, EntryNotes)
            OUTPUT INSERTED.Id
            VALUES (@Name, @Date, @LocationId, @Environment, @HandTimed, @SeasonId, @EntryStatus, @EntryNotes)";
        return await connection.ExecuteScalarAsync<int>(sql, meet);
    }

    public async Task<bool> UpdateAsync(Meet meet)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE Meets
            SET Name = @Name,
                Date = @Date,
                LocationId = @LocationId,
                Environment = @Environment,
                HandTimed = @HandTimed,
                SeasonId = @SeasonId,
                EntryStatus = @EntryStatus,
                EntryNotes = @EntryNotes
            WHERE Id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, meet);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = "DELETE FROM Meets WHERE Id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    public async Task<int> GetPerformanceCountAsync(int meetId)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = "SELECT COUNT(*) FROM Performances WHERE MeetId = @MeetId";
        return await connection.ExecuteScalarAsync<int>(sql, new { MeetId = meetId });
    }

    public async Task<List<Meet>> GetRecentMeetsAsync(int count = 5)
    {
        using var connection = connectionFactory.CreateConnection();
        var sql = $@"
            SELECT TOP {count}
                m.*,
                l.*,
                s.*
            FROM Meets m
            INNER JOIN Locations l ON m.LocationId = l.Id
            INNER JOIN Seasons s ON s.Id = m.SeasonId
            ORDER BY m.Date DESC";

        var meets = await connection.QueryAsync<Meet, Location, Season, Meet>(
            sql,
            (meet, location, season) =>
            {
                meet.Location = location;
                meet.Season = season;
                return meet;
            },
            splitOn: "Id,Id");

        return meets.ToList();
    }
}