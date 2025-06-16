using CloverleafTrack.Models.Enums;

namespace CloverleafTrack.ViewModels;

public class SchoolRecordViewModel
{
    public string EventName { get; set; } = string.Empty;
    public string RecordHolder { get; set; } = string.Empty;
    public string Performance { get; set; } = string.Empty;
    public string MeetName { get; set; } = string.Empty;
    public DateTime MeetDate { get; set; }
    public Gender  Gender { get; set; }
}