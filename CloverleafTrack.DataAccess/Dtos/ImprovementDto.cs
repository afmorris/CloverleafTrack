using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.DataAccess.Dtos;

public class ImprovementDto
{
    public string EventName { get; set; } = string.Empty;
    public string AthleteFirstName { get; set; } = string.Empty;
    public string AthleteLastName { get; set; } = string.Empty;
    public double ImprovementAmount { get; set; }
    public double? PreviousTimeSeconds { get; set; }
    public double? PreviousDistanceInches { get; set; }
    public double? CurrentTimeSeconds { get; set; }
    public double? CurrentDistanceInches { get; set; }
    public Environment Environment { get; set; }
}