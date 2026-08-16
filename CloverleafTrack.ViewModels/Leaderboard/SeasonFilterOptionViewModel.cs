namespace CloverleafTrack.ViewModels.Leaderboard;

/// <summary>
/// A single past-season entry in the event detail page's season scope picker.
/// </summary>
public class SeasonFilterOptionViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
