using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.ViewModels.Athletes;

public class PersonalRecordViewModel
{
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string Performance { get; set; } = string.Empty;
    public Environment Environment { get; set; }
    public DateTime Date { get; set; }
    public string MeetName { get; set; } = string.Empty;
    public int? AllTimeRank { get; set; }
    public int EventCategorySortOrder { get; set; }
    public int EventSortOrder { get; set; }
}