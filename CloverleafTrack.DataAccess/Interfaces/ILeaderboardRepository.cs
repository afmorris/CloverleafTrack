using CloverleafTrack.DataAccess.Dtos;

namespace CloverleafTrack.DataAccess.Interfaces;

public interface ILeaderboardRepository
{
    Task<List<LeaderboardDto>> GetTopPerformancePerEventAsync();
}