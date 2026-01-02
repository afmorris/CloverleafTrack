namespace CloverleafTrack.ViewModels.Home;

public class ImprovementViewModel
{
    public string EventName { get; set; } = string.Empty;
    public string AthleteFirstName { get; set; } = string.Empty;
    public string AthleteLastName { get; set; } = string.Empty;
    public string AthleteFullName => $"{AthleteFirstName} {AthleteLastName}";
    public string AthleteSlug { get; set; } = string.Empty;
    public string ImprovementDisplay { get; set; } = string.Empty;
    public string PreviousPerformance { get; set; } = string.Empty;
    public string CurrentPerformance { get; set; } = string.Empty;
}