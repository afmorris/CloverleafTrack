namespace CloverleafTrack.ViewModels.Meets;

public class MeetPerformanceViewModel
{
    public string AthleteName { get; set; } = string.Empty;
    public string Performance { get; set; } = string.Empty;
    public bool IsPersonalBest { get; set; }
    public bool IsSchoolRecord { get; set; }
    public bool IsSeasonBest { get; set; }
    public int? AllTimeRank { get; set; }
}