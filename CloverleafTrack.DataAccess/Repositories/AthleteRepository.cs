using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using CloverleafTrack.Models.Helpers;
using Dapper;

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
    a.Id, a.FirstName, a.LastName, a.GraduationYear, a.Gender,
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
}