using CloverleafTrack.Models.Enums;
using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.ViewModels.Leaderboard;

public class LeaderboardEventViewModel
{
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string EventKey { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public Environment Environment { get; set; }

    public string AthleteFirstName { get; set; } = string.Empty;
    public string AthleteLastName { get; set; } = string.Empty;
    public string RelayName { get; set; } = string.Empty;
    public string AthleteFullName => $"{AthleteFirstName} {AthleteLastName}";
    public string AthleteSlug { get; set; } = string.Empty;

    public string Performance { get; set; } = string.Empty;
    public DateTime? MeetDate { get; set; }
    public string MeetName { get; set; } = string.Empty;
    public string MeetSlug { get; set; } = string.Empty;
}