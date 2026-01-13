namespace CloverleafTrack.ViewModels.Admin.Dashboard;

public class DataQualityIssueViewModel
{
    public string Type { get; set; } = string.Empty; // "warning" or "error"
    public int Count { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ActionLink { get; set; }
}