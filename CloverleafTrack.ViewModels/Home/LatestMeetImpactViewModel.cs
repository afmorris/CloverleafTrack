using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.ViewModels.Home;

public class LatestMeetImpactViewModel
{
    public string MeetName { get; set; } = string.Empty;
    public string MeetSlug { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public Environment Environment { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public string LocationCity { get; set; } = string.Empty;
    public string LocationState { get; set; } = string.Empty;
    public int TotalPerformances { get; set; }
    public int TotalPRs { get; set; }
    public int TotalSchoolRecords { get; set; }
    public int TopTenAllTimeMarks { get; set; }
    public int UniqueAthletes { get; set; }
}
