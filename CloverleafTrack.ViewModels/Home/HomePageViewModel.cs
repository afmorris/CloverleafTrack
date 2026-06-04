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

    // Latest Meet Impact
    public LatestMeetImpactViewModel? LatestMeetImpact { get; set; }

    // On This Day
    public OnThisDayViewModel? OnThisDay { get; set; }

    // Recent Highlights - Outdoor
    public RecentHighlightViewModel? OutdoorTopPerformance { get; set; }
    public ImprovementViewModel? OutdoorBiggestImprovement { get; set; }
    public BreakoutAthleteViewModel? OutdoorBreakoutAthlete { get; set; }

    // Recent Highlights - Indoor
    public RecentHighlightViewModel? IndoorTopPerformance { get; set; }
    public ImprovementViewModel? IndoorBiggestImprovement { get; set; }
    public BreakoutAthleteViewModel? IndoorBreakoutAthlete { get; set; }

    // Season Leaders - Outdoor
    public List<SeasonLeaderViewModel> BoysOutdoorLeaders { get; set; } = new();
    public List<SeasonLeaderViewModel> GirlsOutdoorLeaders { get; set; } = new();

    // Season Leaders - Indoor
    public List<SeasonLeaderViewModel> BoysIndoorLeaders { get; set; } = new();
    public List<SeasonLeaderViewModel> GirlsIndoorLeaders { get; set; } = new();

    // Upcoming Meets
    public List<UpcomingMeetViewModel> UpcomingMeets { get; set; } = new();

    // Latest meet highlights digest (deterministic, capped at 8)
    public List<HighlightPerformanceViewModel> LatestMeetHighlights { get; set; } = new();
    public string? LatestMeetSlug { get; set; }
}
