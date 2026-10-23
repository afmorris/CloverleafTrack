using CloverleafTrack.Models.Enums;
using CloverleafTrack.ViewModels.Shared;

namespace CloverleafTrack.ViewModels.Athletes;

public class IndividualPerformanceViewModel
{
    public string Performance { get; set; } = string.Empty;
    /// <summary>NM / NH / DNF / DQ rows carry the abbreviation in <see cref="Performance"/> and no RawValue.</summary>
    public ResultStatus ResultStatus { get; set; }
    public bool HasMark => ResultStatus == ResultStatus.Mark;
    public string ResultDescription => ResultStatus.Description();
    public DateTime Date { get; set; }
    public string MeetName { get; set; } = string.Empty;
    /// <summary>PR at the time it was run (BRAIN.md [C118]) — stays true after the athlete improves later.</summary>
    public bool IsPersonalBest { get; set; }
    /// <summary>Still the athlete's best in the event (the current <c>PersonalBest</c> flag). A PR at the time that is not current renders faded.</summary>
    public bool IsCurrentPersonalBest { get; set; }
    /// <summary>Formatted best earlier mark when <see cref="IsPersonalBest"/> and it was not a debut; null otherwise.</summary>
    public string? PreviousBest { get; set; }
    public bool IsSchoolRecord { get; set; }
    /// <summary>Broke the school record when it was set but is no longer the record.</summary>
    public bool WasSchoolRecord { get; set; }
    public bool IsSeasonBest { get; set; }
    public int? AllTimeRank { get; set; }
    /// <summary>Raw numeric value for charting: TimeSeconds for running events, DistanceInches for field events.</summary>
    public double? RawValue { get; set; }
    public string? RelayAthletes { get; set; }
    public bool IsRelay => RelayAthletes != null;
    public string[] RelayMembers => RelayAthletes?.Split("|~|") ?? Array.Empty<string>();

    /// <summary>Empty (HasAttempts == false) unless PerformanceAttempts rows exist for this performance.</summary>
    public PerformanceAttemptSeriesViewModel AttemptSeries { get; set; } = new();

    /// <summary>High jump / pole vault bar ladder; HasBars is false unless bar rows exist.</summary>
    public PerformanceBarSeriesViewModel BarSeries { get; set; } = new();

    /// <summary>Prelim → final chain; HasRounds is false unless the athlete ran at least two rounds.</summary>
    public PerformanceRoundSeriesViewModel RoundSeries { get; set; } = new();
}