namespace CloverleafTrack.ViewModels.Home;

public class SeasonLeaderViewModel
{
    public string EventName { get; set; } = string.Empty;
    public string Performance { get; set; } = string.Empty;
    public string AthleteFirstName { get; set; } = string.Empty;
    public string AthleteLastName { get; set; } = string.Empty;
    public string AthleteFullName => $"{AthleteFirstName} {AthleteLastName}";
    public string AthleteSlug { get; set; } = string.Empty;
    public int? AllTimeRank { get; set; }
}