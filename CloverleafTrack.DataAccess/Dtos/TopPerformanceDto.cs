using CloverleafTrack.Models.Enums;
using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.DataAccess.Dtos;

public class TopPerformanceDto
{
    public int AllTimeRank { get; set; }
    public string EventName { get; set; } = "";
    public string AthleteName { get; set; } = "";
    public double? DistanceInches { get; set; }
    public double? TimeSeconds { get; set; }
    public string MeetName { get; set; } = "";
    public DateTime MeetDate { get; set; }
    public Environment Environment { get; set; }
    public Gender Gender {get; set;}
}