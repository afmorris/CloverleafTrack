namespace CloverleafTrack.Models.Helpers;

/// <summary>Dapper multi-mapping target for the trailing SeasonStartDate/SeasonName columns in AthleteRepository.GetAllWithPerformancesAsync — not a persisted entity.</summary>
public class SeasonMarker
{
    public DateTime SeasonStartDate { get; set; }
    public string SeasonName { get; set; } = string.Empty;
}
