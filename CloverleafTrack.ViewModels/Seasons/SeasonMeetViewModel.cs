namespace CloverleafTrack.ViewModels.Seasons;

public class SeasonMeetViewModel
{
    public DateTime MeetDate { get; set; }
    public string MeetName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int PRCount { get; set; }
    public int SchoolRecordCount { get; set; }
    public string ResultsUrl { get; set; } = string.Empty;
}