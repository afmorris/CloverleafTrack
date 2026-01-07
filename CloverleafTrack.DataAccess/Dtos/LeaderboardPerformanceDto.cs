using CloverleafTrack.Models.Enums;
using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.DataAccess.Dtos;

public class LeaderboardPerformanceDto
{
    public int PerformanceId { get; set; }
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string EventKey { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public Environment Environment { get; set; }
    
    public int? AthleteId { get; set; }
    public string AthleteFirstName { get; set; } = string.Empty;
    public string AthleteLastName { get; set; } = string.Empty;
    public string RelayName { get; set; } = string.Empty;
    public int? GraduationYear { get; set; }
    
    public double? TimeSeconds { get; set; }
    public double? DistanceInches { get; set; }
    
    public int MeetId { get; set; }
    public string MeetName { get; set; } = string.Empty;
    public DateTime MeetDate { get; set; }
    
    public bool PersonalBest { get; set; }
    public bool SchoolRecord { get; set; }
    public int? AllTimeRank { get; set; }
}