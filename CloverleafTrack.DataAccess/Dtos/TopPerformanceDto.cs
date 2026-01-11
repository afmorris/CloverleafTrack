using CloverleafTrack.Models.Enums;
using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.DataAccess.Dtos;

public class TopPerformanceDto
{
    public int AllTimeRank { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string AthleteFirstName { get; set; } = string.Empty;
    public string AthleteLastName { get; set; } = string.Empty;
    public double? DistanceInches { get; set; }
    public double? TimeSeconds { get; set; }
    public string MeetName { get; set; } = string.Empty;
    public DateTime MeetDate { get; set; }
    public Environment Environment { get; set; }
    public Gender Gender { get; set; }
}