namespace CloverleafTrack.ViewModels.Athletes;

public class SeasonPerformanceViewModel
{
    public string SeasonName { get; set; } = string.Empty;
    public int PRCount { get; set; }
    public int SchoolRecordCount { get; set; }
    public List<EventPerformanceGroupViewModel> EventGroups { get; set; } = new();
}