using CloverleafTrack.DataAccess.Dtos;
using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using CloverleafTrack.Models.Helpers;
using Dapper;
using Slugify;

namespace CloverleafTrack.DataAccess.Repositories;

public class AthleteRepository(IDbConnectionFactory connectionFactory) : IAthleteRepository
{
    public async Task<List<Athlete>> GetAllAsync()
    {
        using var connection = connectionFactory.CreateConnection();
        var sql = "SELECT * FROM Athletes";
        var athletes = await connection.QueryAsync<Athlete>(sql);
        return athletes.ToList();
    }

    public async Task<Athlete?> GetByIdAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        var sql = "SELECT * FROM Athletes WHERE Id = @Id";
        return await connection.QuerySingleOrDefaultAsync<Athlete>(sql, new { Id = id });
    }

    public async Task<List<AthleteEventParticipation>> GetAllWithPerformancesAsync()
    {
        using var connection = connectionFactory.CreateConnection();

        var sql = @"
SELECT 
    a.Id, a.FirstName, a.LastName, a.GraduationYear, a.Gender, a.IsActive,
    e.Id AS EventId, e.Id, e.Name, e.EventCategory, e.Environment, e.SortOrder,
    p.Id AS PerformanceId, p.Id, p.DistanceInches, p.TimeSeconds
FROM Athletes a
INNER JOIN Performances p ON p.AthleteId = a.Id
INNER JOIN Events e ON e.Id = p.EventId
ORDER BY a.LastName, a.FirstName;
";
        
        var results = await connection.QueryAsync<Athlete, Event, Performance, AthleteEventParticipation>(
            sql,
            (athlete, eventInfo, performance) => new AthleteEventParticipation
            {
                Athlete = athlete,
                Event = eventInfo,
                Performance = performance
            },
            splitOn: "EventId,PerformanceId"
        );

        return results.ToList();
    }

    public async Task<int> CreateAsync(Athlete athlete)
    {
        using var connection = connectionFactory.CreateConnection();
        var sql = "INSERT INTO Athletes (FirstName, LastName, Gender, GraduationYear) OUTPUT INSERTED.Id VALUES (@FirstName, @LastName, @Gender, @GraduationYear)";
        return await connection.ExecuteScalarAsync<int>(sql, athlete);
    }

    public async Task<bool> UpdateAsync(Athlete athlete)
    {
        using var connection = connectionFactory.CreateConnection();
        var sql = "UPDATE Athletes SET FirstName = @FirstName, LastName = @LastName, Gender = @Gender, GraduationYear = @GraduationYear WHERE Id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, athlete);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(Athlete athlete)
    {
        using var connection = connectionFactory.CreateConnection();
        var sql = "DELETE FROM Athletes WHERE Id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { athlete.Id });
        return rowsAffected > 0;
    }

    public async Task<Athlete?> GetBySlugWithBasicInfoAsync(string slug)
    {
        using var connection = connectionFactory.CreateConnection();

        const string sql = "SELECT * FROM Athletes";
        var athletes = await connection.QueryAsync<Athlete>(sql);
        
        var slugHelper = new SlugHelper();
        return athletes.FirstOrDefault(a => slugHelper.GenerateSlug($"{a.FirstName}-{a.LastName}") == slug);
    }

    public async Task<List<AthletePerformanceDto>> GetAllPerformancesForAthleteAsync(int athleteId)
    {
        using var connection = connectionFactory.CreateConnection();
        
        const string sql = """
                        SELECT
                                p.Id as PerformanceId,
                                e.Id as EventId,
                                e.Name as EventName,
                                e.EventCategorySortOrder,
                                e.EventType,
                                e.Environment,
                                p.TimeSeconds,
                                p.DistanceInches,
                                p.PersonalBest,
                                p.SchoolRecord,
                                p.SeasonBest,
                                (SELECT MIN(lb.Rank) 
                                FROM Leaderboards lb 
                                WHERE lb.PerformanceId = p.Id) as AllTimeRank,
                                m.Date as MeetDate,
                                m.Name as MeetName,
                                s.Name as SeasonName
                        FROM
                                Performances p
                                INNER JOIN Events e ON e.Id = p.EventId
                                INNER JOIN Meets m ON m.Id = p.MeetId
                                INNER JOIN Seasons s ON s.Id = m.SeasonId
                        WHERE
                                p.AthleteId = @AthleteId
                        ORDER BY
                                s.StartDate DESC,
                                e.EventCategorySortOrder,
                                m.Date DESC
                        """;
        
        var performances = await connection.QueryAsync<AthletePerformanceDto>(sql, new { AthleteId = athleteId });
        return performances.ToList();
    }
}