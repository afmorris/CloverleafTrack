using CloverleafTrack.Models;

namespace CloverleafTrack.DataAccess.Interfaces;

/// <summary>Public, read-only access to high jump / pole vault bar ladders — mirrors <see cref="IPerformanceAttemptRepository"/>.</summary>
public interface IPerformanceBarRepository
{
    /// <summary>Batch-loads bar rows for a set of PerformanceIds. Performances without a ladder have no rows in the result.</summary>
    Task<List<PerformanceBar>> GetBarsForPerformancesAsync(IEnumerable<int> performanceIds);
}
