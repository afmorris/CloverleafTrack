using CloverleafTrack.Models.Enums;
using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.Models;

public class Event
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string EventKey { get; set; } = string.Empty;
    public EventType EventType { get; set; }
    public EventCategory? EventCategory { get; set; }
    public Gender? Gender { get; set; }
    public Environment Environment { get; set; }
    public int AthleteCount { get; set; }
    public int SortOrder { get; set; }
    public int EventCategorySortOrder { get; set; }
}