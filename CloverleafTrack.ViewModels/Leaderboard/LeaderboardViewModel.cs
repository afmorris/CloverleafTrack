namespace CloverleafTrack.ViewModels.Leaderboard;

public class LeaderboardViewModel
{
    public List<LeaderboardEventViewModel> BoysOutdoor { get; set; } = new();
    public List<LeaderboardEventViewModel> BoysIndoor { get; set; } = new();
    public List<LeaderboardEventViewModel> GirlsOutdoor { get; set; } = new();
    public List<LeaderboardEventViewModel> GirlsIndoor { get; set; } = new();
    public List<LeaderboardEventViewModel> MixedOutdoor { get; set; } = new();
    public List<LeaderboardEventViewModel> MixedIndoor { get; set; } = new();
}
