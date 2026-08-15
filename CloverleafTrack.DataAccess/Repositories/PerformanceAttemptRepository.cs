using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using Dapper;

namespace CloverleafTrack.DataAccess.Repositories;

public class PerformanceAttemptRepository(IDbConnectionFactory connectionFactory) : IPerformanceAttemptRepository
{
    public async Task<List<PerformanceAttempt>> GetAttemptsForPerformancesAsync(IEnumerable<int> performanceIds)
    {
        var ids = performanceIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return new List<PerformanceAttempt>();
        }

        using var connection = connectionFactory.CreateConnection();
        const string sql = """
                            SELECT *
                            FROM PerformanceAttempts
                            WHERE PerformanceId IN @PerformanceIds
                            ORDER BY PerformanceId, AttemptNumber
                            """;

        var attempts = await connection.QueryAsync<PerformanceAttempt>(sql, new { PerformanceIds = ids });
        return attempts.ToList();
    }
}
