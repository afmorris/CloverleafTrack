namespace CloverleafTrack.ViewModels;

/// <summary>
/// One point on the Roster's per-athlete season trend sparkline (issue #23) — the athlete's best
/// mark in their top event, per season. Pixel coordinates are precomputed server-side against a
/// fixed 88x24 viewBox (CareerChartGeometry), matching the "never do axis math in the view"
/// convention established for the career progression chart (issue #26).
/// </summary>
public class SparklinePointViewModel
{
    public double PixelX { get; set; }
    public double PixelY { get; set; }
    public string SeasonName { get; set; } = string.Empty;
    public string Formatted { get; set; } = string.Empty;
}
