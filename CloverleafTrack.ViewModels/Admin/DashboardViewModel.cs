namespace CloverleafTrack.ViewModels.Admin;

public class DashboardViewModel
{
    public int TotalAthletes { get; set; }
    public int TotalMeets { get; set; }
    public int TotalPerformances { get; set; }
    public int IncompleteMeets { get; set; }

    public List<SeasonProgressViewModel> SeasonProgress { get; set; } = new();
    public List<DataQualityIssueViewModel> DataQualityIssues { get; set; } = new();
}