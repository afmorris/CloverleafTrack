using CloverleafTrack.DataAccess.Dtos;
using CloverleafTrack.DataAccess.Interfaces;
using Dapper;

namespace CloverleafTrack.DataAccess.Repositories;

public class PerformanceRepository(IDbConnectionFactory connectionFactory) : IPerformanceRepository
{
    public async Task<int> CountPRsForSeasonAsync(int seasonId)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = """
                           SELECT
                                COUNT(*)
                           FROM
                                Performances p,
                                Meets m
                           WHERE
                                m.Id = p.MeetId AND
                                m.SeasonId = @SeasonId AND
                                p.PersonalBest = 1
                           """;
        return await connection.ExecuteScalarAsync<int>(sql, new { SeasonId = seasonId });
    }

    public async Task<int> CountAthletesWithPRsForSeasonAsync(int seasonId)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = """
                           SELECT
                                COUNT(DISTINCT p.AthleteId)
                           FROM
                                Performances p,
                                Meets m
                           WHERE
                                m.Id = p.MeetId AND
                                m.SeasonId = @SeasonId AND
                                p.PersonalBest = 1
                           """;
        return await connection.ExecuteScalarAsync<int>(sql, new { SeasonId = seasonId });
    }

    public async Task<int> CountSchoolRecordsBrokenForSeasonAsync(int seasonId)
    {
         using var connection = connectionFactory.CreateConnection();
         const string sql = """
                            SELECT
                                 COUNT(*)
                            FROM
                                 Performances p,
                                 Meets m
                            WHERE
                                 m.Id = p.MeetId AND
                                 m.SeasonId = @SeasonId AND
                                 p.SchoolRecord = 1
                            """;
         return await connection.ExecuteScalarAsync<int>(sql, new { SeasonId = seasonId });
    }

    public async Task<List<TopPerformanceDto>> GetTopPerformancesForSeasonAsync(int seasonId)
    {
         using var connection = connectionFactory.CreateConnection();
         const string sql = """
                            SELECT
                                 l.Rank AS AllTimeRank,
                                 e.Name as EventName,
                                 a.FirstName + ' ' + a.LastName as AthleteName,
                                 p.DistanceInches,
                                 p.TimeSeconds,
                                 m.Name as MeetName,
                                 m.Date as MeetDate
                            FROM
                                 Leaderboards l,
                                 Performances p,
                                 Athletes a,
                                 Meets m,
                                 Events e
                            WHERE
                                 p.Id = l.PerformanceId AND
                                 e.Id = p.AthleteId AND
                                 m.Id = p.MeetId AND
                                 e.Id = p.EventId AND
                                 l.SeasonId = @SeasonId
                            ORDER BY
                                 e.Name,
                                 l.Rank
                            """;
         
         var topPerformances = await connection.QueryAsync<TopPerformanceDto>(sql, new { SeasonId = seasonId });

         return topPerformances.ToList();
    }
}