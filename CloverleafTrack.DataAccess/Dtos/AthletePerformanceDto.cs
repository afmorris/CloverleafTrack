using CloverleafTrack.Models.Enums;
using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.DataAccess.Dtos;

public class AthletePerformanceDto
{
    public int PerformanceId { get; set; }
    public Environment Environment { get; set; }
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public int EventCategorySortOrder { get; set; }
    public int EventSortOrder { get; set; }
    public EventType EventType { get; set; }
    public double? TimeSeconds { get; set; }
    public double? DistanceInches { get; set; }
    public bool PersonalBest { get; set; }
    public bool SchoolRecord { get; set; }
    public bool SeasonBest { get; set; }
    public int? AllTimeRank { get; set; }
    public DateTime MeetDate { get; set; }
    public string MeetName { get; set; } = string.Empty;
    public string SeasonName { get; set; } = string.Empty;
}