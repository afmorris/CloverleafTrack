using CloverleafTrack.ViewModels.Leaderboard;

namespace CloverleafTrack.Services.Interfaces;

public interface ILeaderboardService
{
    Task<LeaderboardViewModel> GetLeaderboardAsync();
    Task<LeaderboardDetailsViewModel?> GetLeaderboardDetailsAsync(string eventKey);
}