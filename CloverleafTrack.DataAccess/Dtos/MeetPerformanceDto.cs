using CloverleafTrack.Models.Enums;

namespace CloverleafTrack.DataAccess.Dtos;

public class MeetPerformanceDto
{
    public int PerformanceId { get; set; }
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public int EventSortOrder { get; set; }
    public EventCategory EventCategory { get; set; }
    public Gender EventGender { get; set; }
    public EventType EventType { get; set; }
    public int? AthleteId { get; set; }
    public string AthleteName { get; set; } = string.Empty;
    public double? TimeSeconds { get; set; }
    public double? DistanceInches { get; set; }
    public bool PersonalBest { get; set; }
    public bool SchoolRecord { get; set; }
    public bool SeasonBest { get; set; }
    public int? AllTimeRank { get; set; }
}