using CloverleafTrack.ViewModels.Leaderboard;

namespace CloverleafTrack.Services.Interfaces;

public interface ILeaderboardService
{
    Task<LeaderboardViewModel> GetLeaderboardAsync();

    /// <summary>
    /// Fetches the event detail page data.
    /// </summary>
    /// <param name="eventKey">The event's URL key.</param>
    /// <param name="scope">"all-time" (default), "season" (current season), or "season-{id}" (a specific past season).</param>
    /// <param name="depth">10 / 25 / 100, or 0 to mean "All" (unbounded). Depth is applied AFTER scope and class filtering.</param>
    Task<LeaderboardDetailsViewModel?> GetLeaderboardDetailsAsync(string eventKey, string? scope = "all-time", int depth = 25);
}