using CloverleafTrack.Models.Enums;

namespace CloverleafTrack.Models;

public class Performance
{
    public int Id { get; set; } 
    public double? DistanceInches { get; set; }
    public double? TimeSeconds { get; set; }
    public string? SortedAthleteHash { get; set; }
    public bool SchoolRecord { get; set; }
    public bool SeasonBest { get; set; }
    public bool PersonalBest { get; set; }
    /// <summary>Beat every earlier mark by this athlete in this event, or is the athlete's first mark there. Permanent — never cleared by a later PR (unlike PersonalBest). BRAIN.md [C118].</summary>
    public bool PRAtTime { get; set; }
    /// <summary>The athlete's best earlier mark (seconds / inches) when PRAtTime is set; null on a debut.</summary>
    public double? PreviousBest { get; set; }
    /// <summary>Broke the school record the day it was set (beat every earlier mark in the event); permanent, unlike SchoolRecord.</summary>
    public bool SRAtTime { get; set; }
    /// <summary>Mark = normal. NM / NH / DNF / DQ rows have no TimeSeconds or DistanceInches. See BRAIN.md [C67].</summary>
    public ResultStatus ResultStatus { get; set; }
    /// <summary>Populated by queries that join Leaderboards. Null when not loaded.</summary>
    public int? AllTimeRank { get; set; }
    /// <summary>
    /// 1-100, higher is better. Populated by queries that join PerformancePercentiles
    /// (rebuilt for every performance by sp_RebuildLeaderboards). Null when not loaded
    /// — do not assume null means "no percentile data exists"; check the query first.
    /// </summary>
    public byte? Percentile { get; set; }

    public int? AthleteId { get; set; }
    public int EventId { get; set; }
    public int MeetId { get; set; }

    public Athlete Athlete { get; set; } = new();
    public Event Event { get; set; } = new();
    public Meet Meet { get; set; } = new();
}