using CloverleafTrack.Models.Enums;
using CloverleafTrack.ViewModels.Shared;

namespace CloverleafTrack.ViewModels.Meets;

public class MeetPerformanceViewModel
{
    public int PerformanceId { get; set; }
    public string AthleteName { get; set; } = string.Empty;
    public string? AthleteSlug { get; set; }
    public string Performance { get; set; } = string.Empty;
    /// <summary>NM / NH / DNF / DQ rows carry the abbreviation in <see cref="Performance"/> and no RawValue.</summary>
    public ResultStatus ResultStatus { get; set; }
    public bool HasMark => ResultStatus == ResultStatus.Mark;
    public string ResultDescription => ResultStatus.Description();
    /// <summary>PR at the time it was run (BRAIN.md [C118]) — stays true after the athlete runs faster later.</summary>
    public bool IsPersonalBest { get; set; }
    /// <summary>Still the athlete's best in the event (the current <c>PersonalBest</c> flag). A PR at the time that is not current renders faded.</summary>
    public bool IsCurrentPersonalBest { get; set; }
    /// <summary>Formatted best earlier mark when <see cref="IsPersonalBest"/> and it was not a debut; null otherwise.</summary>
    public string? PreviousBest { get; set; }
    public bool IsSchoolRecord { get; set; }
    /// <summary>Broke the school record when it was set but is no longer the record (a current record has <see cref="IsSchoolRecord"/>).</summary>
    public bool WasSchoolRecord { get; set; }
    public bool IsSeasonBest { get; set; }
    public int? AllTimeRank { get; set; }

    /// <summary>Empty (HasAttempts == false) unless PerformanceAttempts rows exist for this performance.</summary>
    public PerformanceAttemptSeriesViewModel AttemptSeries { get; set; } = new();

    /// <summary>High jump / pole vault bar ladder; HasBars is false unless bar rows exist.</summary>
    public PerformanceBarSeriesViewModel BarSeries { get; set; } = new();

    /// <summary>Prelim → final chain; HasRounds is false unless the athlete ran at least two rounds.</summary>
    public PerformanceRoundSeriesViewModel RoundSeries { get; set; } = new();

    /// <summary>Raw numeric value for client-side sorting: DistanceInches for field events, TimeSeconds for running events. Never use the formatted Performance string as a sort key.</summary>
    public double? RawValue { get; set; }

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

    public string PlaceLabel => Place switch
    {
        1 => "1st",
        2 => "2nd",
        3 => "3rd",
        _ => $"{Place}th"
    };
}