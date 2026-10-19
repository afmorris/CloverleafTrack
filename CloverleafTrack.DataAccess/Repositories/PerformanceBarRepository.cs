using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using Dapper;

namespace CloverleafTrack.DataAccess.Repositories;

public class PerformanceBarRepository(IDbConnectionFactory connectionFactory) : IPerformanceBarRepository
{
    public async Task<List<PerformanceBar>> GetBarsForPerformancesAsync(IEnumerable<int> performanceIds)
    {
        var ids = performanceIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return new List<PerformanceBar>();
        }

        using var connection = connectionFactory.CreateConnection();
        const string sql = """
                            SELECT *
                            FROM PerformanceBars
                            WHERE PerformanceId IN @PerformanceIds
                            ORDER BY PerformanceId, HeightInches
                            """;

        var bars = await connection.QueryAsync<PerformanceBar>(sql, new { PerformanceIds = ids });
        return bars.ToList();
    }
}
