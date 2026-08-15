using CloverleafTrack.DataAccess.Dtos;

namespace CloverleafTrack.DataAccess.Interfaces;

public interface ILeaderboardRepository
{
    Task<List<LeaderboardDto>> GetTopPerformancePerEventAsync();

    /// <summary>
    /// Returns performances for the given event, optionally scoped to a single season.
    /// <paramref name="seasonId"/> null means all-time (no season filter).
    /// </summary>
    Task<List<LeaderboardPerformanceDto>> GetAllPerformancesForEventAsync(string eventKey, int? seasonId = null);
}