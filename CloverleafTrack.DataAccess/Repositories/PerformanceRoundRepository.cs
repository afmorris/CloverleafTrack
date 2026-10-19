using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using Dapper;

namespace CloverleafTrack.DataAccess.Repositories;

public class PerformanceRoundRepository(IDbConnectionFactory connectionFactory) : IPerformanceRoundRepository
{
    public async Task<List<PerformanceRound>> GetRoundsForPerformancesAsync(IEnumerable<int> performanceIds)
    {
        var ids = performanceIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return new List<PerformanceRound>();
        }

        using var connection = connectionFactory.CreateConnection();
        const string sql = """
                            SELECT *
                            FROM PerformanceRounds
                            WHERE PerformanceId IN @PerformanceIds
                            ORDER BY PerformanceId, Round
                            """;

        var rounds = await connection.QueryAsync<PerformanceRound>(sql, new { PerformanceIds = ids });
        return rounds.ToList();
    }
}
