namespace CloverleafTrack.ViewModels.Performances;

public class TopPerformanceViewModel
{
    public string EventName { get; set; } = string.Empty;
    public string AthleteName { get; set; } = string.Empty;
    public string Performance { get; set; } = string.Empty;
    public string AllTimeRank { get; set; } = string.Empty;
    public string MeetName { get; set; } = string.Empty;
    public DateTime MeetDate { get; set; }
}