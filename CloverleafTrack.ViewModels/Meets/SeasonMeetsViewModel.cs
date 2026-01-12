namespace CloverleafTrack.ViewModels.Meets;

public class SeasonMeetsViewModel
{
    public string SeasonName { get; set; } = string.Empty;
    public int TotalMeets { get; set; }
    public int CompletedMeets { get; set; }
    public int TotalPRs { get; set; }
    public int TotalSchoolRecords { get; set; }
    public bool IsCurrentSeason { get; set; }
    
    public List<MeetListItemViewModel> Meets { get; set; } = new();
}