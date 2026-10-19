using CloverleafTrack.Models;

namespace CloverleafTrack.DataAccess.Interfaces;

/// <summary>Public, read-only access to per-round times — mirrors <see cref="IPerformanceAttemptRepository"/>.</summary>
public interface IPerformanceRoundRepository
{
    /// <summary>Batch-loads round rows for a set of PerformanceIds. Performances without rounds have no rows in the result.</summary>
    Task<List<PerformanceRound>> GetRoundsForPerformancesAsync(IEnumerable<int> performanceIds);
}
