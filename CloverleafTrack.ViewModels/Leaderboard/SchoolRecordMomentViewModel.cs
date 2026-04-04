namespace CloverleafTrack.ViewModels.Leaderboard;

public class SchoolRecordMomentViewModel
{
    public string AthleteName { get; set; } = string.Empty;
    public string AthleteSlug { get; set; } = string.Empty;
    public int? GraduationYear { get; set; }
    public string Performance { get; set; } = string.Empty;
    /// <summary>Raw numeric value (TimeSeconds for running, DistanceInches for field). Used for Chart.js data arrays.</summary>
    public double RawValue { get; set; }
    public string MeetName { get; set; } = string.Empty;
    public string MeetSlug { get; set; } = string.Empty;
    public DateTime MeetDate { get; set; }
    /// <summary>Formatted improvement over the previous record (e.g. "+2' 6.25\"" or "+0.43s"). Null for the first-ever record.</summary>
    public string? ImprovementFormatted { get; set; }
    public bool IsCurrentRecord { get; set; }
}
