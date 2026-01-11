using CloverleafTrack.Models.Enums;
using Slugify;

namespace CloverleafTrack.ViewModels.Performances;

public class TopPerformanceViewModel
{
    public string EventName { get; set; } = string.Empty;
    public string AthleteFirstName { get; set; } = string.Empty;
    public string AthleteLastName { get; set; } = string.Empty;
    public string AthleteName => $"{AthleteFirstName} {AthleteLastName}";
    public string AthleteSlug
    {
        get
        {
            var helper = new SlugHelper();
            return helper.GenerateSlug($"{AthleteFirstName}-{AthleteLastName}");
        }
    }
    public string Performance { get; set; } = string.Empty;
    public string AllTimeRank { get; set; } = string.Empty;
    public string MeetName { get; set; } = string.Empty;
    public string MeetSlug
    {
        get
        {
            var helper = new SlugHelper();
            return helper.GenerateSlug(MeetName);
        }
    }
    public DateTime MeetDate { get; set; }
    public Gender Gender { get; set; }
}