using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.ViewModels.Meets;

public class MeetDetailsViewModel
{
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public string LocationCity { get; set; } = string.Empty;
    public string LocationState { get; set; } = string.Empty;
    public Environment Environment { get; set; }
    public bool HandTimed { get; set; }
    public string SeasonName { get; set; } = string.Empty;
    
    public int TotalPerformances { get; set; }
    public int TotalPRs { get; set; }
    public int TotalSchoolRecords { get; set; }
    public int UniqueAthletes { get; set; }
    
    public List<MeetEventGroupViewModel> BoysEvents { get; set; } = new();
    public List<MeetEventGroupViewModel> GirlsEvents { get; set; } = new();
}