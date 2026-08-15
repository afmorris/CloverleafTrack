namespace CloverleafTrack.ViewModels.Admin.Performances;

/// <summary>
/// One of the 6 attempt-series input slots on the admin performance entry form.
/// Bound as PerformanceEntryViewModel.Attempts[0..5]. A slot is empty (ignored on
/// save) unless DistanceInput has text, IsFoul, or IsPass is set.
/// </summary>
public class PerformanceAttemptInputViewModel
{
    public int AttemptNumber { get; set; }
    public string? DistanceInput { get; set; }
    public bool IsFoul { get; set; }
    public bool IsPass { get; set; }
}
