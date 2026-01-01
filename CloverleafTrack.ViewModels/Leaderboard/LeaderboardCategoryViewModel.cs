using CloverleafTrack.Models.Enums;

namespace CloverleafTrack.ViewModels.Leaderboard;

public class LeaderboardCategoryViewModel
{
    public EventCategory Category { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public List<LeaderboardEventViewModel> Events { get; set; } = new();
}