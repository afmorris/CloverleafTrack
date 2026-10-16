using CloverleafTrack.Models;

namespace CloverleafTrack.ViewModels.Meets;

public class MeetScoreViewModel
{
    public double Score { get; set; }
    public MeetScoreBasis Basis { get; set; }
    public double? Peak { get; set; }
    public double? Depth { get; set; }
    public double? Improvement { get; set; }
    public int PRs { get; set; }
    public double ExpectedPRs { get; set; }
    public int EligibleAthleteEvents { get; set; }

    /// <summary>True for any basis other than Full — such scores are not comparable with full scores.</summary>
    public bool IsPartial => Basis != MeetScoreBasis.Full;

    public string BasisLabel => Basis switch
    {
        MeetScoreBasis.PeakOnly => "Peak only",
        MeetScoreBasis.ImprovementOnly => "Improvement only",
        _ => string.Empty
    };

    public string BasisExplanation => Basis switch
    {
        MeetScoreBasis.PeakOnly => "Meets before the 2020-21 season only have scoring performances recorded, so this score reflects the best marks alone and is not comparable to modern scores.",
        MeetScoreBasis.ImprovementOnly => "JV meets are scored on improvement only, since percentiles compare against all-time varsity marks.",
        _ => string.Empty
    };

    /// <summary>Tailwind classes for the score badge, tiered by score.</summary>
    public string BadgeClasses => Score switch
    {
        >= 85 => "bg-amber-100 text-amber-800 dark:bg-amber-900/40 dark:text-amber-300",
        >= 75 => "bg-emerald-100 text-emerald-800 dark:bg-emerald-900/40 dark:text-emerald-300",
        >= 60 => "bg-blue-100 text-blue-800 dark:bg-blue-900/40 dark:text-blue-300",
        _ => "bg-gray-100 text-gray-600 dark:bg-gray-700 dark:text-gray-300"
    };
}
