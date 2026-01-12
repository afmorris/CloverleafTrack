using Environment = CloverleafTrack.Models.Enums.Environment;
using Slugify;

namespace CloverleafTrack.ViewModels.Home;

public class RecentHighlightViewModel
{
    public string Performance { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public string AthleteFirstName { get; set; } = string.Empty;
    public string AthleteLastName { get; set; } = string.Empty;
    public string AthleteFullName => $"{AthleteFirstName} {AthleteLastName}";
    public string AthleteSlug { get; set; } = string.Empty;
    public string MeetName { get; set; } = string.Empty;
    public string MeetSlug
    {
        get
        {
            var helper = new SlugHelper();
            return helper.GenerateSlug(MeetName);
        }
    }
    public DateTime Date { get; set; }
    public bool IsPersonalBest { get; set; }
    public bool IsSchoolRecord { get; set; }
    public Environment Environment { get; set; }
}