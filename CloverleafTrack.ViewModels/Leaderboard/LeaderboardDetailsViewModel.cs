using CloverleafTrack.Models.Enums;
using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.ViewModels.Leaderboard;

public class LeaderboardDetailsViewModel
{
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string EventKey { get; set; } = string.Empty;
    public bool IsRelayEvent { get; set;}
    public Gender Gender { get; set; }
    public Environment Environment { get; set; }
    public string GenderLabel => Gender switch
    {
        Gender.Male => "Boys",
        Gender.Female => "Girls",
        Gender.Mixed => "Mixed",
        _ => "Unknown"
    };
    
    public bool IsFieldEvent { get; set; }

    // All performances for this event, within the current scope, depth-limited, class = "all"
    public List<LeaderboardPerformanceViewModel> AllPerformances { get; set; } = new();

    // All performances within the current scope, depth-limited, keyed by class — "Freshman"/"Sophomore"/"Junior"/"Senior"
    public Dictionary<string, List<LeaderboardPerformanceViewModel>> ClassAllPerformances { get; set; } = new();

    // Only PRs (best performance per athlete) — shown when class filter is "all"
    public List<LeaderboardPerformanceViewModel> PersonalRecordsOnly { get; set; } = new();

    // Best performance per athlete within each class — keyed by "Freshman"/"Sophomore"/"Junior"/"Senior"
    public Dictionary<string, List<LeaderboardPerformanceViewModel>> ClassPersonalRecords { get; set; } = new();

    // School record progression over time, sorted best-first (distance desc / time asc)
    // NOTE: computed from the currently-scoped performance set (see LeaderboardService) — for a
    // season scope this is the progression *within that season's data*, not necessarily including
    // the all-time record if it was not set in-scope.
    public List<SchoolRecordMomentViewModel> SchoolRecordProgression { get; set; } = new();

    // -------------------------------------------------------------------
    // Scope / Depth control state (Issue #3 — Event page depth)
    // -------------------------------------------------------------------

    /// <summary>"all-time" (default), "season" (current season), or "season-{id}" (a specific past season).</summary>
    public string ScopeValue { get; set; } = "all-time";

    /// <summary>10 / 25 / 100, or 0 to mean "All" (unbounded).</summary>
    public int DepthValue { get; set; } = 25;

    /// <summary>Total performance count in the current scope (all classes), before depth-limiting — drives the "Show all" affordance.</summary>
    public int TotalPerformanceCount { get; set; }

    /// <summary>Id of the current season, if one exists — used to render the "This Season" scope control.</summary>
    public int? CurrentSeasonId { get; set; }

    /// <summary>Past (non-current) seasons for the season-scope dropdown, ordered most-recent-first.</summary>
    public List<SeasonFilterOptionViewModel> SeasonOptions { get; set; } = new();
}
