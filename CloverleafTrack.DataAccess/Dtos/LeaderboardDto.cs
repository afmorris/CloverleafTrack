using CloverleafTrack.Models.Enums;
using Slugify;
using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.DataAccess.Dtos;

public class LeaderboardDto
{
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string EventKey { get; set; } = string.Empty;
    public EventCategory EventCategory { get; set; }
    public int EventCategorySortOrder { get; set; }
    public int EventSortOrder { get; set; }
    public Gender Gender { get; set; }
    public Environment Environment { get; set; }

    public int PerformanceId { get; set; }
    public double? TimeSeconds { get; set; }
    public double? DistanceInches { get; set; }

    public int AthleteId { get; set; }
    public string AthleteFirstName { get; set; } = string.Empty;
    public string AthleteLastName { get; set; } = string.Empty;
    public string RelayName { get; set; } = string.Empty;
    public string AthleteFullName => $"{AthleteFirstName} {AthleteLastName}";
    public string AthleteSlug
    {
        get
        {
            var helper = new SlugHelper();
            return helper.GenerateSlug($"{AthleteFirstName}-{AthleteLastName}");
        }
    }

    public DateTime MeetDate { get; set; }
    public string MeetName { get; set; } = string.Empty;
    public string MeetSlug
    {
        get
        {
            var helper = new SlugHelper();
            return helper.GenerateSlug(MeetName);
        }
    }
}