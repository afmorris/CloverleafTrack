using CloverleafTrack.Models.Enums;

namespace CloverleafTrack.DataAccess.Dtos;

public class MeetEntryDto
{
    public int Id { get; set; }
    public int MeetId { get; set; }
    public int EventId { get; set; }
    public int? AthleteId { get; set; }
    public int? PerformanceId { get; set; }

    public string EventName { get; set; } = string.Empty;
    public EventType EventType { get; set; }
    public EventCategory EventCategory { get; set; }
    public Gender EventGender { get; set; }
    public int EventAthleteCount { get; set; }
    public int EventSortOrder { get; set; }
    public int EventCategorySortOrder { get; set; }

    public string? AthleteFirstName { get; set; }
    public string? AthleteLastName { get; set; }
    public Gender? AthleteGender { get; set; }

    /// <summary>|~|-separated "FirstName LastName" strings for relay entries. Null for individual entries.</summary>
    public string? RelayAthleteNames { get; set; }

    /// <summary>Populated when PerformanceId is set (joined from Performances).</summary>
    public double? TimeSeconds { get; set; }
    public double? DistanceInches { get; set; }

    public bool IsRelay => AthleteId == null;

    public string AthleteDisplayName => IsRelay
        ? (RelayAthleteNames?.Replace("|~|", " / ") ?? "Relay")
        : $"{AthleteFirstName} {AthleteLastName}".Trim();
}
