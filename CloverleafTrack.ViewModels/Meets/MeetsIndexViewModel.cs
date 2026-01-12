namespace CloverleafTrack.ViewModels.Meets;

public class MeetsIndexViewModel
{
    // Overall stats
    public int TotalMeets { get; set; }
    public int TotalPRs { get; set; }
    public int TotalSchoolRecords { get; set; }
    public int TotalSeasons { get; set; }
    
    // Meets grouped by season
    public List<SeasonMeetsViewModel> Seasons { get; set; } = new();
}