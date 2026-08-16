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