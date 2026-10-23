using CloverleafTrack.DataAccess.Dtos;
using CloverleafTrack.DataAccess.Interfaces;
using Dapper;

namespace CloverleafTrack.DataAccess.Repositories;

public class SeasonWrapRepository(IDbConnectionFactory connectionFactory) : ISeasonWrapRepository
{
    public async Task<List<SeasonWrapMarkDto>> GetMarksAsync(int seasonId)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = """
                           SELECT
                               p.Id AS PerformanceId,
                               p.AthleteId,
                               a.FirstName AS AthleteFirstName,
                               a.LastName AS AthleteLastName,
                               a.GraduationYear,
                               (SELECT STRING_AGG(a2.FirstName + ' ' + a2.LastName, '|~|')
                                FROM PerformanceAthletes pa
                                INNER JOIN Athletes a2 ON a2.Id = pa.AthleteId
                                WHERE pa.PerformanceId = p.Id AND p.AthleteId IS NULL) AS RelayAthletes,
                               e.Id AS EventId,
                               e.Name AS EventName,
                               e.Gender AS EventGender,
                               e.Environment,
                               e.EventType,
                               p.TimeSeconds,
                               p.DistanceInches,
                               p.PRAtTime,
                               p.PreviousBest,
                               p.SRAtTime,
                               (SELECT MIN(lb.Rank) FROM Leaderboards lb WHERE lb.PerformanceId = p.Id) AS AllTimeRank,
                               (SELECT pp.Percentile FROM PerformancePercentiles pp WHERE pp.PerformanceId = p.Id) AS Percentile,
                               m.Id AS MeetId,
                               m.Name AS MeetName,
                               m.Date AS MeetDate,
                               m.MeetType
                           FROM Performances p
                           INNER JOIN Events e ON e.Id = p.EventId
                           INNER JOIN Meets m ON m.Id = p.MeetId
                           LEFT JOIN Athletes a ON a.Id = p.AthleteId
                           WHERE m.SeasonId = @SeasonId
                             AND p.ResultStatus = 0
                           ORDER BY m.Date, p.Id
                           """;
        return (await connection.QueryAsync<SeasonWrapMarkDto>(sql, new { SeasonId = seasonId })).ToList();
    }

    public async Task<List<SeasonWrapPriorBestDto>> GetPriorBestsAsync(int seasonId, DateTime seasonStart)
    {
        using var connection = connectionFactory.CreateConnection();
        // A subquery (not IN @ids): the athlete list can be large and Dapper expands a list into one parameter per element.
        const string sql = """
                           SELECT p.AthleteId, p.EventId, MIN(p.TimeSeconds) AS BestTime, MAX(p.DistanceInches) AS BestDistance
                           FROM Performances p
                           INNER JOIN Meets m ON m.Id = p.MeetId
                           WHERE p.ResultStatus = 0
                             AND p.AthleteId IS NOT NULL
                             AND m.Date < @SeasonStart
                             AND p.AthleteId IN (SELECT p2.AthleteId
                                                 FROM Performances p2
                                                 INNER JOIN Meets m2 ON m2.Id = p2.MeetId
                                                 WHERE m2.SeasonId = @SeasonId AND p2.AthleteId IS NOT NULL)
                           GROUP BY p.AthleteId, p.EventId
                           """;
        return (await connection.QueryAsync<SeasonWrapPriorBestDto>(sql, new { SeasonId = seasonId, SeasonStart = seasonStart })).ToList();
    }

    public async Task<List<SeasonWrapTeamResultDto>> GetTeamResultsAsync(int seasonId)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = """
                           SELECT
                               t.MeetId,
                               m.Name AS MeetName,
                               m.Date AS MeetDate,
                               m.MeetType,
                               t.Gender,
                               s.Name AS OpponentName,
                               t.OurScore,
                               t.OpponentScore
                           FROM MeetTeamResults t
                           INNER JOIN Meets m ON m.Id = t.MeetId
                           LEFT JOIN MeetParticipants mp ON mp.Id = t.OpponentMeetParticipantId
                           LEFT JOIN Schools s ON s.Id = mp.SchoolId
                           WHERE m.SeasonId = @SeasonId
                             AND t.Deleted = 0
                             AND t.OurScore IS NOT NULL
                             AND t.OpponentScore IS NOT NULL
                             AND m.MeetType IN (1, 2)
                           ORDER BY m.Date, t.Gender, s.Name
                           """;
        return (await connection.QueryAsync<SeasonWrapTeamResultDto>(sql, new { SeasonId = seasonId })).ToList();
    }
}
