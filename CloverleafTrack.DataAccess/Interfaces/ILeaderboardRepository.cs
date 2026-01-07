using CloverleafTrack.DataAccess.Dtos;

namespace CloverleafTrack.DataAccess.Interfaces;

public interface ILeaderboardRepository
{
    Task<List<LeaderboardDto>> GetTopPerformancePerEventAsync();
    Task<List<LeaderboardPerformanceDto>> GetAllPerformancesForEventAsync(string eventKey);
}