namespace CloverleafTrack.ViewModels.Leaderboard;

public class LeaderboardPerformanceViewModel
{
    public int Rank { get; set; }
    public string AthleteName { get; set; } = string.Empty;
    public string AthleteSlug { get; set; } = string.Empty;
    public string Performance { get; set; } = string.Empty;
    public string MeetName { get; set; } = string.Empty;
    public string MeetSlug { get; set; } = string.Empty;
    public DateTime MeetDate { get; set; }
    public int? GraduationYear { get; set; }
    public bool IsSchoolRecord { get; set; }
    /// <summary>True if this performance set the school record at the time it was performed (not necessarily the current record).</summary>
    public bool WasRecordAtTime { get; set; }
}