using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.ViewModels.Admin;

public class MeetSeasonGroupViewModel
{
    public string SeasonName { get; set; } = string.Empty;
    public Environment Environment { get; set; }
    public int TotalMeets { get; set; }
    public int EnteredMeets { get; set; }
    public int TotalPerformances { get; set; }
    public List<MeetListViewModel> Meets { get; set; } = new();
}