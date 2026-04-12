using CloverleafTrack.Models.Enums;

namespace CloverleafTrack.DataAccess.Dtos;

/// <summary>
/// Flat row returned by the season scoring query. One row per athlete per placing.
/// For relay performances the query expands each PerformanceAthlete into its own row,
/// so relay members are represented individually here.
/// </summary>
public class ScoringDataDto
{
    public int PlacingId { get; set; }
    public int MeetId { get; set; }
    public string MeetName { get; set; } = string.Empty;
    public DateTime MeetDate { get; set; }
    public int PerformanceId { get; set; }
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public EventType EventType { get; set; }
    public EventCategory EventCategory { get; set; }
    public int EventAthleteCount { get; set; }
    public int AthleteId { get; set; }
    public string AthleteFirstName { get; set; } = string.Empty;
    public string AthleteLastName { get; set; } = string.Empty;
    public Gender AthleteGender { get; set; }
    public int? MeetParticipantId { get; set; }
    public string? OpponentSchoolName { get; set; }
    public int Place { get; set; }
    public decimal FullPoints { get; set; }
    public decimal SplitPoints { get; set; }

    public bool IsRelay => EventType is EventType.RunningRelay or EventType.FieldRelay
                                      or EventType.JumpRelay or EventType.ThrowsRelay;

    public string AthleteFullName => $"{AthleteFirstName} {AthleteLastName}".Trim();
}
