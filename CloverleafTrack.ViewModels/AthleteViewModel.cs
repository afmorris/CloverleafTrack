using CloverleafTrack.Models.Enums;
using Slugify;

namespace CloverleafTrack.ViewModels;

public class AthleteViewModel
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string AthleteSlug
    {
        get
        {
            var helper = new SlugHelper();
            return helper.GenerateSlug($"{FirstName}-{LastName}");
        }
    }
    public int GraduationYear { get; set; }
    public Gender Gender { get; set; }
    public string Class { get; set; } = string.Empty;
    public List<EventParticipationViewModel> EventsInCategory { get; set; } = new();
    public List<EventCategory> Categories { get; set; } = new();

    /// <summary>Best event by percentile (issue #23's Roster "Top Event" column) — not simply the first event by SortOrder, which is what this used to fall back to. Null only when the athlete has no percentile-eligible performances at all.</summary>
    public EventParticipationViewModel? TopEvent { get; set; }

    /// <summary>Per-season best mark in TopEvent, oldest first — the Roster's 88x24 sparkline column.</summary>
    public List<SparklinePointViewModel> SeasonTrend { get; set; } = new();
    public string SparklinePolyline => string.Join(" ", SeasonTrend.Select(p => $"{p.PixelX:0.##},{p.PixelY:0.##}"));
}
