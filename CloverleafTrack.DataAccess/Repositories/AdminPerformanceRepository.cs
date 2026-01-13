using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using Dapper;

namespace CloverleafTrack.DataAccess.Repositories;

public class AdminPerformanceRepository(IDbConnectionFactory connectionFactory) : IAdminPerformanceRepository
{
    public async Task<List<Performance>> GetAllPerformancesAsync()
    {
        using var connection = connectionFactory.CreateConnection();

        const string sql = @"
            SELECT 
                p.*,
                a.*,
                e.*,
                m.*
            FROM Performances p
            LEFT JOIN Athletes a ON a.Id = p.AthleteId
            INNER JOIN Events e ON e.Id = p.EventId
            INNER JOIN Meets m ON m.Id = p.MeetId
            ORDER BY m.Date DESC";

        var performances = await connection.QueryAsync<Performance, Athlete, Event, Meet, Performance>(
            sql,
            (performance, athlete, evt, meet) =>
            {
                performance.Athlete = athlete ?? new Athlete();
                performance.Event = evt;
                performance.Meet = meet;
                return performance;
            },
            splitOn: "Id,Id,Id");

        return performances.ToList();
    }

    public async Task<List<Performance>> GetPerformancesByMeetAsync(int meetId)
    {
        using var connection = connectionFactory.CreateConnection();

        const string sql = @"
            SELECT 
                p.*,
                a.*,
                e.*,
                m.*
            FROM Performances p
            LEFT JOIN Athletes a ON a.Id = p.AthleteId
            INNER JOIN Events e ON e.Id = p.EventId
            INNER JOIN Meets m ON m.Id = p.MeetId
            WHERE p.MeetId = @MeetId
            ORDER BY e.EventCategorySortOrder, e.SortOrder";

        var performances = await connection.QueryAsync<Performance, Athlete, Event, Meet, Performance>(
            sql,
            (performance, athlete, evt, meet) =>
            {
                performance.Athlete = athlete ?? new Athlete();
                performance.Event = evt;
                performance.Meet = meet;
                return performance;
            },
            new { MeetId = meetId },
            splitOn: "Id,Id,Id");

        return performances.ToList();
    }

    public async Task<Performance?> GetPerformanceByIdAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();

        const string sql = @"
            SELECT 
                p.*,
                a.*,
                e.*,
                m.*
            FROM Performances p
            LEFT JOIN Athletes a ON a.Id = p.AthleteId
            INNER JOIN Events e ON e.Id = p.EventId
            INNER JOIN Meets m ON m.Id = p.MeetId
            WHERE p.Id = @Id";

        var performances = await connection.QueryAsync<Performance, Athlete, Event, Meet, Performance>(
            sql,
            (performance, athlete, evt, meet) =>
            {
                performance.Athlete = athlete ?? new Athlete();
                performance.Event = evt;
                performance.Meet = meet;
                return performance;
            },
            new { Id = id },
            splitOn: "Id,Id,Id");

        return performances.FirstOrDefault();
    }

    public async Task<int> CreatePerformanceAsync(Performance performance)
    {
        using var connection = connectionFactory.CreateConnection();

        const string sql = @"
            INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SortedAthleteHash, SchoolRecord, SeasonBest, PersonalBest)
            OUTPUT INSERTED.Id
            VALUES (@AthleteId, @MeetId, @EventId, @DistanceInches, @TimeSeconds, @SortedAthleteHash, @SchoolRecord, @SeasonBest, @PersonalBest)";

        return await connection.ExecuteScalarAsync<int>(sql, performance);
    }

    public async Task<bool> UpdatePerformanceAsync(Performance performance)
    {
        using var connection = connectionFactory.CreateConnection();

        const string sql = @"
            UPDATE Performances 
            SET AthleteId = @AthleteId,
                MeetId = @MeetId,
                EventId = @EventId,
                DistanceInches = @DistanceInches,
                TimeSeconds = @TimeSeconds,
                SortedAthleteHash = @SortedAthleteHash,
                SchoolRecord = @SchoolRecord,
                SeasonBest = @SeasonBest,
                PersonalBest = @PersonalBest
            WHERE Id = @Id";

        var rowsAffected = await connection.ExecuteAsync(sql, performance);
        return rowsAffected > 0;
    }

    public async Task<bool> DeletePerformanceAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = "DELETE FROM Performances WHERE Id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    public async Task<Performance?> CheckDuplicatePerformanceAsync(int meetId, int eventId, int? athleteId)
    {
        using var connection = connectionFactory.CreateConnection();

        const string sql = @"
            SELECT * FROM Performances 
            WHERE MeetId = @MeetId 
                AND EventId = @EventId 
                AND AthleteId = @AthleteId";

        return await connection.QuerySingleOrDefaultAsync<Performance>(sql, new { MeetId = meetId, EventId = eventId, AthleteId = athleteId });
    }

    public async Task<Performance?> GetAthleteCurrentPRAsync(int athleteId, int eventId)
    {
        using var connection = connectionFactory.CreateConnection();

        const string sql = @"
            SELECT TOP 1 p.*, e.*
            FROM Performances p
            INNER JOIN Events e ON e.Id = p.EventId
            WHERE p.AthleteId = @AthleteId 
                AND p.EventId = @EventId
            ORDER BY 
                CASE 
                    WHEN e.EventType IN (0, 2) THEN p.DistanceInches 
                    ELSE -p.TimeSeconds 
                END DESC";

        var performances = await connection.QueryAsync<Performance, Event, Performance>(
            sql,
            (performance, evt) =>
            {
                performance.Event = evt;
                return performance;
            },
            new { AthleteId = athleteId, EventId = eventId },
            splitOn: "Id");

        return performances.FirstOrDefault();
    }

    public async Task<List<int>> GetRelayAthleteIdsAsync(int performanceId)
    {
        using var connection = connectionFactory.CreateConnection();

        const string sql = @"
            SELECT AthleteId 
            FROM PerformanceAthletes 
            WHERE PerformanceId = @PerformanceId
            ORDER BY Id";

        var athleteIds = await connection.QueryAsync<int>(sql, new { PerformanceId = performanceId });
        return athleteIds.ToList();
    }

    public async Task<bool> AddRelayAthleteAsync(int performanceId, int athleteId)
    {
        using var connection = connectionFactory.CreateConnection();

        const string sql = @"
            INSERT INTO PerformanceAthletes (PerformanceId, AthleteId)
            VALUES (@PerformanceId, @AthleteId)";

        var rowsAffected = await connection.ExecuteAsync(sql, new { PerformanceId = performanceId, AthleteId = athleteId });
        return rowsAffected > 0;
    }

    public async Task<bool> RemoveAllRelayAthletesAsync(int performanceId)
    {
        using var connection = connectionFactory.CreateConnection();

        const string sql = "DELETE FROM PerformanceAthletes WHERE PerformanceId = @PerformanceId";
        var rowsAffected = await connection.ExecuteAsync(sql, new { PerformanceId = performanceId });
        return rowsAffected > 0;
    }
}