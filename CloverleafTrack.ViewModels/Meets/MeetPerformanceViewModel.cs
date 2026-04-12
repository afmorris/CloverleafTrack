namespace CloverleafTrack.ViewModels.Meets;

public class MeetPerformanceViewModel
{
    public string AthleteName { get; set; } = string.Empty;
    public string? AthleteSlug { get; set; }
    public string Performance { get; set; } = string.Empty;
    public bool IsPersonalBest { get; set; }
    public bool IsSchoolRecord { get; set; }
    public bool IsSeasonBest { get; set; }
    public int? AllTimeRank { get; set; }

    /// <summary>
    /// Placings for this performance. Invitational = one entry; Double Dual = up to two (one per opponent).
    /// Empty if no placing has been recorded.
    /// </summary>
    public List<PerformancePlacingViewModel> Placings { get; set; } = new();

    /// <summary>True when this performance has at least one placing recorded.</summary>
    public bool HasPlacing => Placings.Count > 0;
}

public class PerformancePlacingViewModel
{
    public int Place { get; set; }
    public decimal FullPoints { get; set; }
    public decimal SplitPoints { get; set; }

    /// <summary>Opponent school name for dual/double dual placings. Null for invitationals.</summary>
    public string? OpponentSchoolName { get; set; }

    public string MedalEmoji => Place switch
    {
        1 => "🥇",
        2 => "🥈",
        3 => "🥉",
        _ => $"{Place}th"
    };
}