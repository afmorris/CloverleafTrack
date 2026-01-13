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
    
    // All performances for this event
    public List<LeaderboardPerformanceViewModel> AllPerformances { get; set; } = new();
    
    // Only PRs (best performance per athlete)
    public List<LeaderboardPerformanceViewModel> PersonalRecordsOnly { get; set; } = new();
}
