namespace CloverleafTrack.ViewModels.Home;

public class BreakoutAthleteViewModel
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string Slug { get; set; } = string.Empty;
    public int PRCount { get; set; }
    public string Class { get; set; } = string.Empty;
}