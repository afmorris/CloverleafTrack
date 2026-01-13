namespace CloverleafTrack.ViewModels.Admin;
public class PerformanceSummaryViewModel
{
    public int Id { get; set; }
    public string AthleteName { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public string PerformanceDisplay { get; set; } = string.Empty;
    public double? TimeSeconds { get; set; }
    public double? DistanceInches { get; set; }
    public bool IsPersonalBest { get; set; }
    public bool IsSchoolRecord { get; set; }
}