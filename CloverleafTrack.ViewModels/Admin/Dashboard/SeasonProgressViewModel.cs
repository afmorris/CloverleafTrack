namespace CloverleafTrack.ViewModels.Admin.Dashboard;

public class SeasonProgressViewModel
{
    public string SeasonName { get; set; } = string.Empty;
    public int TotalMeets { get; set; }
    public int EnteredMeets { get; set; }
    public int PercentComplete => TotalMeets > 0 ? (EnteredMeets * 100 / TotalMeets) : 0;
    public bool IsCurrentSeason { get; set; }
}