namespace CloverleafTrack.ViewModels.Shared;

/// <summary>
/// Model for the shared "Pct" + "All-time rank" cell partial (issue #19). Kept separate from
/// any one page's PR/performance ViewModel so the same partial can be reused wherever a
/// percentile + rank pair needs to render (roster today; event pages / search later).
/// </summary>
public class PercentileRankViewModel
{
    public byte? Percentile { get; set; }
    public int? AllTimeRank { get; set; }
    public int? EventMarkCount { get; set; }
    public bool HasData => Percentile.HasValue;
}
