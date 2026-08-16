using CloverleafTrack.Models;

namespace CloverleafTrack.DataAccess.Interfaces;

/// <summary>
/// Public, read-only access to recorded attempt series — used by the public-facing
/// services (Meet, Athlete, Leaderboard) to attach attempt data to already-loaded
/// performances. See IAdminPerformanceRepository for the admin save/load path.
/// </summary>
public interface IPerformanceAttemptRepository
{
    /// <summary>
    /// Batch-loads attempt rows for a set of PerformanceIds in one round trip. Performances
    /// with no recorded series simply have no rows in the result — callers should treat an
    /// absent PerformanceId as "no attempts recorded" rather than an error.
    /// </summary>
    Task<List<PerformanceAttempt>> GetAttemptsForPerformancesAsync(IEnumerable<int> performanceIds);
}
