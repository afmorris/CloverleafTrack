using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.ViewModels.Home;

public class UpcomingMeetViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public Environment Environment { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
}