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
}