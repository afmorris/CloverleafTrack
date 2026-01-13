using Environment = CloverleafTrack.Models.Enums.Environment;
using CloverleafTrack.Models.Enums;

namespace CloverleafTrack.ViewModels.Admin;

public class MeetListViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public string LocationCity { get; set; } = string.Empty;
    public string LocationState { get; set; } = string.Empty;
    public Environment Environment { get; set; }
    public MeetEntryStatus EntryStatus { get; set; }
    public string SeasonName { get; set; } = string.Empty;
    public int PerformanceCount { get; set; }
}