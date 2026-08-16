namespace CloverleafTrack.ViewModels.Shared;

/// <summary>
/// One attempt for display. Distances are raw inches — views format them with
/// PerformanceFormatHelper.FormatDistance (Web project), not here, since the
/// ViewModels project does not reference Web.Utilities.
/// </summary>
public class PerformanceAttemptViewModel
{
    public int AttemptNumber { get; set; }
    public double? DistanceInches { get; set; }
    public bool IsFoul { get; set; }
    public bool IsPass { get; set; }
    public bool IsValid => !IsFoul && !IsPass && DistanceInches.HasValue;

    /// <summary>True for the single best (max) valid attempt in the series.</summary>
    public bool IsBest { get; set; }
}

/// <summary>
/// The full attempt series (up to 6 attempts) for one performance, plus the derived
/// stats shown in the compact strip. Attached to a performance-display ViewModel
/// (MeetPerformanceViewModel, IndividualPerformanceViewModel, LeaderboardPerformanceViewModel)
/// as an empty instance (HasAttempts == false) by default — every existing display
/// renders identically to today unless attempt rows actually exist for a performance.
/// </summary>
public class PerformanceAttemptSeriesViewModel
{
    public List<PerformanceAttemptViewModel> Attempts { get; set; } = new();

    /// <summary>False when no PerformanceAttempts rows exist for this performance — views must render nothing in that case.</summary>
    public bool HasAttempts => Attempts.Count > 0;

    private IEnumerable<PerformanceAttemptViewModel> ValidAttempts => Attempts.Where(a => a.IsValid);

    public int ValidAttemptCount => ValidAttempts.Count();

    /// <summary>Average of valid (non-foul, non-pass) attempts, in inches. Null when there are none.</summary>
    public double? AverageValidInches
    {
        get
        {
            var values = ValidAttempts.Select(a => a.DistanceInches!.Value).ToList();
            return values.Count == 0 ? null : values.Average();
        }
    }

    /// <summary>Best valid attempt minus worst valid attempt, in inches. Null when there are fewer than 2 valid attempts to spread.</summary>
    public double? SpreadInches
    {
        get
        {
            var values = ValidAttempts.Select(a => a.DistanceInches!.Value).ToList();
            return values.Count == 0 ? null : values.Max() - values.Min();
        }
    }
}
