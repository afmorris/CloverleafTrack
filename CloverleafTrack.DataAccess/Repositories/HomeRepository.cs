using CloverleafTrack.DataAccess.Dtos;
using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models.Enums;
using Dapper;
using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.DataAccess.Repositories;

public class HomeRepository(IDbConnectionFactory connectionFactory) : IHomeRepository
{
    public async Task<HomePageStatsDto> GetHomePageStatsAsync(int currentSeasonId)
    {
        using var connection = connectionFactory.CreateConnection();

        const string sql = """
                           SELECT 
                               (SELECT COUNT(*) FROM Performances p
                                INNER JOIN Meets m ON m.Id = p.MeetId
                                WHERE m.SeasonId = @SeasonId AND p.PersonalBest = 1) AS TotalPRsThisSeason,
                               
                               (SELECT COUNT(*) FROM Performances p
                                INNER JOIN Meets m ON m.Id = p.MeetId
                                WHERE m.SeasonId = @SeasonId AND p.SchoolRecord = 1) AS SchoolRecordsBroken,
                               
                               (SELECT COUNT(*) FROM Athletes WHERE IsActive = 1) AS ActiveAthletes,
                               
                               (SELECT COUNT(*) FROM Meets WHERE SeasonId = @SeasonId AND EntryStatus = 3) AS MeetsCompleted,
                               
                               (SELECT COUNT(*) FROM Meets WHERE SeasonId = @SeasonId) AS TotalMeetsThisSeason
                           """;

        return await connection.QueryFirstAsync<HomePageStatsDto>(sql, new { SeasonId = currentSeasonId });
    }

    public async Task<OnThisDayDto?> GetPerformanceOnThisDayAsync(int month, int day)
    {
        using var connection = connectionFactory.CreateConnection();

        const string sql = """
                           SELECT TOP 1
                               e.Name AS EventName,
                               p.TimeSeconds,
                               p.DistanceInches,
                               a.FirstName AS AthleteFirstName,
                               a.LastName AS AthleteLastName,
                               m.Name AS MeetName,
                               m.Environment AS MeetEnvironment,
                               m.Date,
                               p.SchoolRecord AS IsSchoolRecord,
                               (SELECT MIN(lb.Rank) FROM Leaderboards lb WHERE lb.PerformanceId = p.Id) AS AllTimeRank
                           FROM Performances p
                           INNER JOIN Events e ON e.Id = p.EventId
                           INNER JOIN Athletes a ON a.Id = p.AthleteId
                           INNER JOIN Meets m ON m.Id = p.MeetId
                           WHERE MONTH(m.Date) = @Month AND DAY(m.Date) = @Day
                           ORDER BY 
                               p.SchoolRecord DESC,
                               CASE WHEN (SELECT MIN(lb.Rank) FROM Leaderboards lb WHERE lb.PerformanceId = p.Id) <= 3 THEN 0 ELSE 1 END,
                               m.Date DESC
                           """;

        return await connection.QueryFirstOrDefaultAsync<OnThisDayDto>(sql, new { Month = month, Day = day });
    }

    public async Task<RecentHighlightDto?> GetRecentTopPerformanceAsync(int currentSeasonId, Environment environment)
    {
        using var connection = connectionFactory.CreateConnection();

        // Try to get a performance from the last 7 days first
        const string recentSql = """
                                 SELECT TOP 1
                                     e.Name AS EventName,
                                     e.Environment,
                                     p.TimeSeconds,
                                     p.DistanceInches,
                                     a.FirstName AS AthleteFirstName,
                                     a.LastName AS AthleteLastName,
                                     m.Name AS MeetName,
                                     m.Date,
                                     p.PersonalBest AS IsPersonalBest,
                                     p.SchoolRecord AS IsSchoolRecord
                                 FROM Performances p
                                 INNER JOIN Events e ON e.Id = p.EventId
                                 INNER JOIN Athletes a ON a.Id = p.AthleteId
                                 INNER JOIN Meets m ON m.Id = p.MeetId
                                 WHERE m.SeasonId = @SeasonId 
                                   AND e.Environment = @Environment
                                   AND m.Date >= DATEADD(day, -7, GETDATE())
                                 ORDER BY 
                                     p.SchoolRecord DESC,
                                     p.PersonalBest DESC,
                                     m.Date DESC
                                 """;

        var recent = await connection.QueryFirstOrDefaultAsync<RecentHighlightDto>(recentSql, new { SeasonId = currentSeasonId, Environment = environment });

        if (recent != null)
        {
            return recent;
        }

        // Fall back to season best if no recent performances
        const string seasonBestSql = """
                                     SELECT TOP 1
                                         e.Name AS EventName,
                                         e.Environment,
                                         p.TimeSeconds,
                                         p.DistanceInches,
                                         a.FirstName AS AthleteFirstName,
                                         a.LastName AS AthleteLastName,
                                         m.Name AS MeetName,
                                         m.Date,
                                         p.PersonalBest AS IsPersonalBest,
                                         p.SchoolRecord AS IsSchoolRecord
                                     FROM Performances p
                                     INNER JOIN Events e ON e.Id = p.EventId
                                     INNER JOIN Athletes a ON a.Id = p.AthleteId
                                     INNER JOIN Meets m ON m.Id = p.MeetId
                                     INNER JOIN Leaderboards lb ON lb.PerformanceId = p.Id AND lb.Rank = 1
                                     WHERE m.SeasonId = @SeasonId
                                       AND e.Environment = @Environment
                                     ORDER BY m.Date DESC
                                     """;

        return await connection.QueryFirstOrDefaultAsync<RecentHighlightDto>(seasonBestSql, new { SeasonId = currentSeasonId, Environment = environment });
    }

    public async Task<ImprovementDto?> GetBiggestImprovementThisSeasonAsync(int currentSeasonId, Environment environment)
    {
        using var connection = connectionFactory.CreateConnection();

        const string sql = """
                           WITH CurrentSeasonBests AS (
                               SELECT 
                                   p.AthleteId,
                                   p.EventId,
                                   e.Name AS EventName,
                                   e.EventType,
                                   e.Environment,
                                   a.FirstName AS AthleteFirstName,
                                   a.LastName AS AthleteLastName,
                                   MIN(p.TimeSeconds) AS BestTime,
                                   MAX(p.DistanceInches) AS BestDistance
                               FROM Performances p
                               INNER JOIN Events e ON e.Id = p.EventId
                               INNER JOIN Athletes a ON a.Id = p.AthleteId
                               INNER JOIN Meets m ON m.Id = p.MeetId
                               WHERE m.SeasonId = @SeasonId
                                 AND e.Environment = @Environment
                               GROUP BY p.AthleteId, p.EventId, e.Name, e.EventType, e.Environment, a.FirstName, a.LastName
                           ),
                           PreviousSeasonBests AS (
                               SELECT 
                                   p.AthleteId,
                                   p.EventId,
                                   MIN(p.TimeSeconds) AS BestTime,
                                   MAX(p.DistanceInches) AS BestDistance
                               FROM Performances p
                               INNER JOIN Events e ON e.Id = p.EventId
                               INNER JOIN Meets m ON m.Id = p.MeetId
                               WHERE m.SeasonId < @SeasonId
                                 AND e.Environment = @Environment
                               GROUP BY p.AthleteId, p.EventId
                           )
                           SELECT TOP 1
                               cs.EventName,
                               cs.Environment,
                               cs.AthleteFirstName,
                               cs.AthleteLastName,
                               CASE 
                                   WHEN cs.EventType IN (0, 2) THEN ps.BestTime - cs.BestTime
                                   ELSE cs.BestDistance - ps.BestDistance
                               END AS ImprovementAmount,
                               ps.BestTime AS PreviousTimeSeconds,
                               ps.BestDistance AS PreviousDistanceInches,
                               cs.BestTime AS CurrentTimeSeconds,
                               cs.BestDistance AS CurrentDistanceInches
                           FROM CurrentSeasonBests cs
                           INNER JOIN PreviousSeasonBests ps ON ps.AthleteId = cs.AthleteId AND ps.EventId = cs.EventId
                           WHERE 
                               (cs.EventType IN (0, 2) AND ps.BestTime > cs.BestTime) OR
                               (cs.EventType IN (1, 3) AND cs.BestDistance > ps.BestDistance)
                           ORDER BY ImprovementAmount DESC
                           """;

        return await connection.QueryFirstOrDefaultAsync<ImprovementDto>(sql, new { SeasonId = currentSeasonId, Environment = environment });
    }

    public async Task<BreakoutAthleteDto?> GetBreakoutAthleteAsync(int currentSeasonId, Environment environment)
    {
        using var connection = connectionFactory.CreateConnection();

        const string sql = """
                           SELECT TOP 1
                               a.FirstName,
                               a.LastName,
                               a.GraduationYear,
                               COUNT(*) AS PRCount,
                               @Environment AS Environment
                           FROM Performances p
                           INNER JOIN Athletes a ON a.Id = p.AthleteId
                           INNER JOIN Events e ON e.Id = p.EventId
                           INNER JOIN Meets m ON m.Id = p.MeetId
                           WHERE m.SeasonId = @SeasonId 
                             AND p.PersonalBest = 1
                             AND e.Environment = @Environment
                           GROUP BY a.Id, a.FirstName, a.LastName, a.GraduationYear
                           ORDER BY PRCount DESC
                           """;

        return await connection.QueryFirstOrDefaultAsync<BreakoutAthleteDto>(sql, new { SeasonId = currentSeasonId, Environment = environment });
    }

    public async Task<List<SeasonLeaderDto>> GetSeasonLeadersAsync(Gender gender, int currentSeasonId, Environment environment)
    {
        using var connection = connectionFactory.CreateConnection();

        const string sql = """
                           WITH RankedPerformances AS (
                               SELECT 
                                   e.Id AS EventId,
                                   e.Name AS EventName,
                                   e.EventCategorySortOrder,
                                   e.SortOrder,
                                   p.TimeSeconds,
                                   p.DistanceInches,
                                   a.FirstName AS AthleteFirstName,
                                   a.LastName AS AthleteLastName,
                                   lb.Rank AS AllTimeRank,
                                   ROW_NUMBER() OVER (PARTITION BY e.Id ORDER BY lb.Rank ASC) AS EventRank
                               FROM Performances p
                               INNER JOIN Events e ON e.Id = p.EventId
                               INNER JOIN Athletes a ON a.Id = p.AthleteId
                               INNER JOIN Meets m ON m.Id = p.MeetId
                               INNER JOIN Leaderboards lb ON lb.PerformanceId = p.Id
                               WHERE e.Gender = @Gender
                                 AND e.Environment = @Environment
                                 AND m.SeasonId = @SeasonId
                                 AND e.EventCategory IS NOT NULL
                                 AND lb.Rank IS NOT NULL
                           )
                           SELECT TOP 3
                               EventName,
                               TimeSeconds,
                               DistanceInches,
                               AthleteFirstName,
                               AthleteLastName,
                               AllTimeRank
                           FROM RankedPerformances
                           WHERE EventRank = 1
                           ORDER BY AllTimeRank ASC, EventCategorySortOrder, SortOrder
                           """;

        var results = await connection.QueryAsync<SeasonLeaderDto>(sql, new { Gender = gender, SeasonId = currentSeasonId, Environment = environment });
        return results.ToList();
    }

    public async Task<List<UpcomingMeetDto>> GetUpcomingMeetsAsync(int count = 5)
    {
        using var connection = connectionFactory.CreateConnection();

        const string sql = """
                           SELECT TOP (@Count)
                               m.Id,
                               m.Name,
                               m.Date,
                               m.Environment,
                               COALESCE(l.Name, '') AS Location
                           FROM Meets m
                           LEFT JOIN Locations l ON l.Id = m.LocationId
                           WHERE m.Date > GETDATE()
                           ORDER BY m.Date ASC
                           """;

        var results = await connection.QueryAsync<UpcomingMeetDto>(sql, new { Count = count });
        return results.ToList();
    }
}