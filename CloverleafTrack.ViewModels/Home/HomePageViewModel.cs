using CloverleafTrack.Models.Enums;
using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.ViewModels.Home;

public class HomePageViewModel
{
    // Season at a Glance
    public int TotalPRsThisSeason { get; set; }
    public int SchoolRecordsBroken { get; set; }
    public int ActiveAthletes { get; set; }
    public int MeetsCompleted { get; set; }
    public int TotalMeetsThisSeason { get; set; }
    
    // On This Day
    public OnThisDayViewModel? OnThisDay { get; set; }
    
    // Recent Highlights
    public RecentHighlightViewModel? TopPerformance { get; set; }
    public ImprovementViewModel? BiggestImprovement { get; set; }
    public BreakoutAthleteViewModel? BreakoutAthlete { get; set; }
    
    // Season Leaders
    public List<SeasonLeaderViewModel> BoysLeaders { get; set; } = new();
    public List<SeasonLeaderViewModel> GirlsLeaders { get; set; } = new();
    
    // Upcoming Meets
    public List<UpcomingMeetViewModel> UpcomingMeets { get; set; } = new();
}