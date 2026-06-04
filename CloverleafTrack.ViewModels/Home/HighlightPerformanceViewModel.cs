namespace CloverleafTrack.ViewModels.Home;

public class HighlightPerformanceViewModel
{
    public string EventName { get; set; } = string.Empty;
    public string AthleteName { get; set; } = string.Empty;
    public string? AthleteSlug { get; set; }
    public string Performance { get; set; } = string.Empty;
    public bool IsSchoolRecord { get; set; }
    public bool IsPersonalBest { get; set; }
    public int? AllTimeRank { get; set; }
    public int? BestPlace { get; set; }
    public string? PlaceContext { get; set; }
}
