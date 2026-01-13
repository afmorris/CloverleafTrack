using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using Dapper;

namespace CloverleafTrack.DataAccess.Repositories;

public class AdminPerformanceRepository(IDbConnectionFactory connectionFactory) : IAdminPerformanceRepository
{
    public async Task<Performance?> GetByIdAsync(int id)
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

    public async Task<List<Performance>> GetAllWithDetailsAsync()
    {
        using var connection = connectionFactory.CreateConnection();
        
        // First get all performances with their basic details
        const string sql = @"
            SELECT 
                p.*,
                a.*,
                e.*,
                m.*,
                l.*,
                s.*
            FROM Performances p
            LEFT JOIN Athletes a ON a.Id = p.AthleteId
            INNER JOIN Events e ON e.Id = p.EventId
            INNER JOIN Meets m ON m.Id = p.MeetId
            INNER JOIN Locations l ON l.Id = m.LocationId
            INNER JOIN Seasons s ON s.Id = m.SeasonId
            ORDER BY m.Date DESC, e.SortOrder";
        
        var performanceDict = new Dictionary<int, Performance>();
        
        await connection.QueryAsync<Performance, Athlete, Event, Meet, Location, Season, Performance>(
            sql,
            (performance, athlete, evt, meet, location, season) =>
            {
                if (!performanceDict.TryGetValue(performance.Id, out var existingPerformance))
                {
                    existingPerformance = performance;
                    existingPerformance.Athlete = athlete ?? new Athlete();
                    existingPerformance.Event = evt;
                    existingPerformance.Meet = meet;
                    existingPerformance.Meet.Location = location;
                    existingPerformance.Meet.Season = season;
                    performanceDict.Add(performance.Id, existingPerformance);
                }
                return existingPerformance;
            },
            splitOn: "Id,Id,Id,Id,Id");
        
        // Now get relay athletes for performances that don't have an AthleteId
        var relayPerformances = performanceDict.Values.Where(p => !p.AthleteId.HasValue).ToList();
        
        if (relayPerformances.Any())
        {
            const string relayAthletesSql = @"
                SELECT 
                    pa.PerformanceId,
                    STRING_AGG(CONCAT(a.LastName, ', ', a.FirstName), '; ') as RelayAthletes
                FROM PerformanceAthletes pa
                INNER JOIN Athletes a ON a.Id = pa.AthleteId
                WHERE pa.PerformanceId IN @PerformanceIds
                GROUP BY pa.PerformanceId";
            
            var relayData = await connection.QueryAsync<(int PerformanceId, string RelayAthletes)>(
                relayAthletesSql,
                new { PerformanceIds = relayPerformances.Select(p => p.Id).ToList() });
            
            foreach (var relay in relayData)
            {
                if (performanceDict.TryGetValue(relay.PerformanceId, out var performance))
                {
                    // Store relay athlete names in a temporary property or use Athlete.LastName as a workaround
                    performance.Athlete = new Athlete 
                    { 
                        FirstName = "",
                        LastName = relay.RelayAthletes 
                    };
                }
            }
        }
        
        return performanceDict.Values.ToList();
    }

    public async Task<int> CreateAsync(Performance performance)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SortedAthleteHash, SchoolRecord, SeasonBest, PersonalBest)
            OUTPUT INSERTED.Id
            VALUES (@AthleteId, @MeetId, @EventId, @DistanceInches, @TimeSeconds, @SortedAthleteHash, @SchoolRecord, @SeasonBest, @PersonalBest)";
        
        var performanceId = await connection.ExecuteScalarAsync<int>(sql, performance);
        
        // Recalculate leaderboards for this event
        await connection.ExecuteAsync("EXEC sp_RebuildLeaderboards");
        
        return performanceId;
    }

    public async Task<bool> UpdateAsync(Performance performance)
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
        
        // Recalculate leaderboards for this event
        await connection.ExecuteAsync("EXEC sp_RebuildLeaderboards");
        
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        
        // Get the performance details before deleting (for leaderboard recalculation)
        const string getPerformanceSql = "SELECT EventId FROM Performances WHERE Id = @Id";
        var eventId = await connection.QuerySingleOrDefaultAsync<int?>(getPerformanceSql, new { Id = id });
        
        // Delete related records in the correct order (respecting foreign key constraints)
        
        // 1. Delete from Leaderboards (references Performances)
        await connection.ExecuteAsync("DELETE FROM Leaderboards WHERE PerformanceId = @Id", new { Id = id });
        
        // 2. Delete from PerformanceAthletes (references Performances)
        await connection.ExecuteAsync("DELETE FROM PerformanceAthletes WHERE PerformanceId = @Id", new { Id = id });
        
        // 3. Delete the performance itself
        const string sql = "DELETE FROM Performances WHERE Id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        
        // 4. Recalculate leaderboards for the affected event
        if (eventId.HasValue)
        {
            await connection.ExecuteAsync("EXEC sp_RebuildLeaderboards");
        }
        
        return rowsAffected > 0;
    }

    public async Task<List<Performance>> GetPerformancesForMeetAsync(int meetId)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            SELECT 
                p.*,
                a.*,
                e.*
            FROM Performances p
            LEFT JOIN Athletes a ON a.Id = p.AthleteId
            INNER JOIN Events e ON e.Id = p.EventId
            WHERE p.MeetId = @MeetId
            ORDER BY e.EventCategorySortOrder, e.SortOrder";
        
        var performances = await connection.QueryAsync<Performance, Athlete, Event, Performance>(
            sql,
            (performance, athlete, evt) =>
            {
                performance.Athlete = athlete ?? new Athlete();
                performance.Event = evt;
                return performance;
            },
            new { MeetId = meetId },
            splitOn: "Id,Id");
        
        return performances.ToList();
    }

    public async Task<Performance?> GetSimilarPerformanceAsync(int meetId, int eventId, int? athleteId)
    {
        using var connection = connectionFactory.CreateConnection();
        
        if (athleteId.HasValue)
        {
            // Check for individual performance
            const string sql = @"
                SELECT * FROM Performances
                WHERE MeetId = @MeetId
                AND EventId = @EventId
                AND AthleteId = @AthleteId";
            
            return await connection.QuerySingleOrDefaultAsync<Performance>(sql, new 
            { 
                MeetId = meetId, 
                EventId = eventId, 
                AthleteId = athleteId.Value 
            });
        }
        
        return null;
    }

    public async Task<Performance?> GetBestPerformanceForAthleteEventAsync(int athleteId, int eventId)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            SELECT TOP 1 
                p.*,
                e.*
            FROM Performances p
            INNER JOIN Events e ON e.Id = p.EventId
            WHERE p.AthleteId = @AthleteId
            AND p.EventId = @EventId
            ORDER BY 
                CASE WHEN e.EventType IN (0, 2, 4, 5) THEN p.DistanceInches ELSE 0 END DESC,
                CASE WHEN e.EventType IN (1, 3) THEN p.TimeSeconds ELSE 999999 END ASC";
        
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

    public async Task<int> CreatePerformanceAthleteAsync(int performanceId, int athleteId)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO PerformanceAthletes (PerformanceId, AthleteId)
            OUTPUT INSERTED.Id
            VALUES (@PerformanceId, @AthleteId)";
        return await connection.ExecuteScalarAsync<int>(sql, new { PerformanceId = performanceId, AthleteId = athleteId });
    }

    public async Task<bool> DeletePerformanceAthletesAsync(int performanceId)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = "DELETE FROM PerformanceAthletes WHERE PerformanceId = @PerformanceId";
        var rowsAffected = await connection.ExecuteAsync(sql, new { PerformanceId = performanceId });
        return rowsAffected > 0;
    }

    public async Task<List<int>> GetAthleteIdsForPerformanceAsync(int performanceId)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = "SELECT AthleteId FROM PerformanceAthletes WHERE PerformanceId = @PerformanceId";
        var athleteIds = await connection.QueryAsync<int>(sql, new { PerformanceId = performanceId });
        return athleteIds.ToList();
    }
}