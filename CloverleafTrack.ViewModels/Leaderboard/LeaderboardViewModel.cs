namespace CloverleafTrack.ViewModels.Leaderboard;

public class LeaderboardViewModel
{
    public List<LeaderboardCategoryViewModel> BoysOutdoorCategories { get; set; } = new();
    public List<LeaderboardCategoryViewModel> BoysIndoorCategories { get; set; } = new();
    public List<LeaderboardCategoryViewModel> GirlsOutdoorCategories { get; set; } = new();
    public List<LeaderboardCategoryViewModel> GirlsIndoorCategories { get; set; } = new();
    public List<LeaderboardCategoryViewModel> MixedOutdoorCategories { get; set; } = new();
    public List<LeaderboardCategoryViewModel> MixedIndoorCategories { get; set; } = new();
}