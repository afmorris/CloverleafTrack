using CloverleafTrack.ViewModels.Shared;

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
    /// <summary>The athlete's class (Freshman/Sophomore/Junior/Senior) at the time this performance was set. Null for relays or unknown graduation years.</summary>
    public string? ClassAtTimeOfPerformance { get; set; }
    /// <summary>Raw numeric value for sort: DistanceInches for field events, TimeSeconds for running events.</summary>
    public double? RawValue { get; set; }

    /// <summary>1-100, higher is better. Drives the Mark cell tint (issue #21) — event pages are single-event tables, so percentile is always comparable here.</summary>
    public byte? Percentile { get; set; }

    /// <summary>Empty (HasAttempts == false) unless PerformanceAttempts rows exist for this performance.</summary>
    public PerformanceAttemptSeriesViewModel AttemptSeries { get; set; } = new();
}