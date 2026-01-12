using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.ViewModels.Meets;

public class MeetListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public Environment Environment { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public string LocationCity { get; set; } = string.Empty;
    public string LocationState { get; set; } = string.Empty;
    
    public int AthleteCount { get; set; }
    public int PerformanceCount { get; set; }
    public int PRCount { get; set; }
    public int SchoolRecordCount { get; set; }
    
    public bool IsUpcoming => Date > DateTime.Now;
}