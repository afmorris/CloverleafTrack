using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.DataAccess.Dtos;

public class OnThisDayDto
{
    public string EventName { get; set; } = string.Empty;
    public double? TimeSeconds { get; set; }
    public double? DistanceInches { get; set; }
    public string AthleteFirstName { get; set; } = string.Empty;
    public string AthleteLastName { get; set; } = string.Empty;
    public string MeetName { get; set; } = string.Empty;
    public Environment MeetEnvironment { get; set; }
    public DateTime Date { get; set; }
    public bool IsSchoolRecord { get; set; }
    public int? AllTimeRank { get; set; }
}
