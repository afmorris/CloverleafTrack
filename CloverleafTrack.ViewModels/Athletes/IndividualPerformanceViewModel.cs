namespace CloverleafTrack.ViewModels.Athletes;

public class IndividualPerformanceViewModel
{
    public string Performance { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string MeetName { get; set; } = string.Empty;
    public bool IsPersonalBest { get; set; }
    public bool IsSchoolRecord { get; set; }
    public bool IsSeasonBest { get; set; }
    public int? AllTimeRank { get; set; }
    /// <summary>Raw numeric value for charting: TimeSeconds for running events, DistanceInches for field events.</summary>
    public double? RawValue { get; set; }
    public string? RelayAthletes { get; set; }
    public bool IsRelay => RelayAthletes != null;
    public string[] RelayMembers => RelayAthletes?.Split("|~|") ?? Array.Empty<string>();
}