namespace CloverleafTrack.DataAccess.Dtos;

public class SeasonLeaderDto
{
    public string EventName { get; set; } = string.Empty;
    public double? TimeSeconds { get; set; }
    public double? DistanceInches { get; set; }
    public string AthleteFirstName { get; set; } = string.Empty;
    public string AthleteLastName { get; set; } = string.Empty;
    public int? AllTimeRank { get; set; }
}
