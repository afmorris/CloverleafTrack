using Slugify;

namespace CloverleafTrack.ViewModels.Seasons;

public class SeasonMeetViewModel
{
    public DateTime MeetDate { get; set; }
    public string MeetName { get; set; } = string.Empty;
    public string Slug {
        get
        {
            var helper = new SlugHelper();
            return helper.GenerateSlug(MeetName);
        }
    }
    public string Location { get; set; } = string.Empty;
    public int PRCount { get; set; }
    public int SchoolRecordCount { get; set; }
    public string ResultsUrl { get; set; } = string.Empty;
}