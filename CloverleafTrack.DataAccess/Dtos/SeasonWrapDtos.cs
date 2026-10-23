using CloverleafTrack.Models.Enums;
using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.DataAccess.Dtos;

/// <summary>One recorded mark (ResultStatus = Mark) in a season, with everything the season wrap needs about it. BRAIN.md [C123].</summary>
public class SeasonWrapMarkDto
{
    public int PerformanceId { get; set; }
    public int? AthleteId { get; set; }
    public string? AthleteFirstName { get; set; }
    public string? AthleteLastName { get; set; }
    public int? GraduationYear { get; set; }
    /// <summary>|~|-separated "First Last" for relay performances; null for individuals.</summary>
    public string? RelayAthletes { get; set; }
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public Gender EventGender { get; set; }
    public Environment Environment { get; set; }
    public EventType EventType { get; set; }
    public double? TimeSeconds { get; set; }
    public double? DistanceInches { get; set; }
    public bool PRAtTime { get; set; }
    public double? PreviousBest { get; set; }
    public bool SRAtTime { get; set; }
    /// <summary>Current all-time rank (Leaderboards), not rank at the time.</summary>
    public int? AllTimeRank { get; set; }
    public byte? Percentile { get; set; }
    public int MeetId { get; set; }
    public string MeetName { get; set; } = string.Empty;
    public DateTime MeetDate { get; set; }
    public MeetType MeetType { get; set; }
}

/// <summary>An athlete's best mark in an event strictly before the season started (for "improved over last season").</summary>
public class SeasonWrapPriorBestDto
{
    public int AthleteId { get; set; }
    public int EventId { get; set; }
    public double? BestTime { get; set; }
    public double? BestDistance { get; set; }
}

public class SeasonWrapTeamResultDto
{
    public int MeetId { get; set; }
    public string MeetName { get; set; } = string.Empty;
    public DateTime MeetDate { get; set; }
    public MeetType MeetType { get; set; }
    public Gender Gender { get; set; }
    public string? OpponentName { get; set; }
    public decimal? OurScore { get; set; }
    public decimal? OpponentScore { get; set; }
}
