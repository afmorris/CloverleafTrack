using Environment = CloverleafTrack.Models.Enums.Environment;
using Slugify;

namespace CloverleafTrack.ViewModels.Home;

public class OnThisDayViewModel
{
    public string EventName { get; set; } = string.Empty;
    public string Performance { get; set; } = string.Empty;
    public string AthleteFirstName { get; set; } = string.Empty;
    public string AthleteLastName { get; set; } = string.Empty;
    public string AthleteFullName => $"{AthleteFirstName} {AthleteLastName}";
    public string AthleteSlug
    {
        get
        {
            var helper = new SlugHelper();
            return helper.GenerateSlug($"{AthleteFirstName}-{AthleteLastName}");
        }
    }
    public string MeetName { get; set; } = string.Empty;
    public Environment MeetEnvironment { get; set; }
    public string MeetSlug
    {
        get
        {
            var helper = new SlugHelper();
            return helper.GenerateSlug(MeetName);
        }
    }
    public DateTime Date { get; set; }
    public bool IsSchoolRecord { get; set; }
    public int? AllTimeRank { get; set; }
}