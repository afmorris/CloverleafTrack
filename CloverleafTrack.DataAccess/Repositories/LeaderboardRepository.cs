using CloverleafTrack.DataAccess.Dtos;
using CloverleafTrack.DataAccess.Interfaces;
using Dapper;

namespace CloverleafTrack.DataAccess.Repositories;

public class LeaderboardRepository(IDbConnectionFactory connectionFactory) : ILeaderboardRepository
{
    public async Task<List<LeaderboardDto>> GetTopPerformancePerEventAsync()
    {
        using var connection = connectionFactory.CreateConnection();

        const string sql = """
                           SELECT 
                               e.Id as EventId,
                               e.Name as EventName,
                               e.EventKey,
                               e.EventCategory,
                               e.EventCategorySortOrder,
                               e.EventType,
                               e.SortOrder as EventSortOrder,
                               e.Gender,
                               e.Environment,
                               p.Id as PerformanceId,
                               p.TimeSeconds,
                               p.DistanceInches,
                               a.Id as AthleteId,
                               a.FirstName as AthleteFirstName,
                               a.LastName as AthleteLastName,
                               CASE 
                                     WHEN p.AthleteId IS NULL THEN 
                                          -- For relays, concatenate all athlete names
                                          (SELECT STRING_AGG(a2.FirstName + ' ' + a2.LastName, '|~|')
                                          FROM PerformanceAthletes pa
                                          INNER JOIN Athletes a2 ON a2.Id = pa.AthleteId
                                          WHERE pa.PerformanceId = p.Id)
                                     ELSE 
                                          ''
                               END as RelayName,
                               m.Date as MeetDate,
                               m.Name as MeetName
                           FROM Events e
                           LEFT JOIN Leaderboards lb ON lb.EventId = e.Id AND lb.Rank = 1
                           LEFT JOIN Performances p ON p.Id = lb.PerformanceId
                           LEFT JOIN Athletes a ON a.Id = p.AthleteId
                           LEFT JOIN Meets m ON m.Id = p.MeetId
                           WHERE
                            e.EventCategory IS NOT NULL
                           ORDER BY
                            e.Gender,
                            e.Environment DESC,
                            e.EventCategorySortOrder,
                            e.SortOrder
                           """;

        var results = await connection.QueryAsync<LeaderboardDto>(sql);
        return results.ToList();
    }
}