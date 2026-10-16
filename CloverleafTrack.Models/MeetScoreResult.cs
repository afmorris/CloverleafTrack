namespace CloverleafTrack.Models;

public enum MeetScoreBasis
{
    /// <summary>Peak + Depth + Improvement, weighted.</summary>
    Full = 0,
    /// <summary>Pre-complete-data-era meet: only scoring performances exist, so the score is Peak alone. Not comparable to Full scores.</summary>
    PeakOnly = 1,
    /// <summary>JV meet: percentiles compare against varsity history, so the score is Improvement alone.</summary>
    ImprovementOnly = 2
}

public class MeetScoreResult
{
    public int MeetId { get; set; }

    /// <summary>0-100.</summary>
    public double Score { get; set; }
    public MeetScoreBasis Basis { get; set; }

    /// <summary>0-100 sub-scores; null when not part of this meet's basis.</summary>
    public double? Peak { get; set; }
    public double? Depth { get; set; }
    public double? Improvement { get; set; }

    public int PRs { get; set; }
    public double ExpectedPRs { get; set; }
    public int EligibleAthleteEvents { get; set; }
}
