using CloverleafTrack.Models.Enums;

namespace CloverleafTrack.ViewModels.Scoring;

public class AthleteScoreRowViewModel
{
    public int AthleteId { get; set; }
    public string AthleteFirstName { get; set; } = string.Empty;
    public string AthleteLastName { get; set; } = string.Empty;
    public string AthleteSlug { get; set; } = string.Empty;
    public string AthleteFullName => $"{AthleteFirstName} {AthleteLastName}".Trim();

    // ── Totals ────────────────────────────────────────────────
    public decimal TotalFullPoints { get; set; }
    public decimal TotalSplitPoints { get; set; }

    // ── Running vs Field ─────────────────────────────────────
    public decimal RunningFullPoints { get; set; }
    public decimal RunningSplitPoints { get; set; }
    public decimal FieldFullPoints { get; set; }
    public decimal FieldSplitPoints { get; set; }

    // ── Individual vs Relay ───────────────────────────────────
    public decimal IndividualPoints { get; set; }  // same for full/split — individuals have no adjustment
    public decimal RelayFullPoints { get; set; }
    public decimal RelaySplitPoints { get; set; }

    // ── By Event Category ────────────────────────────────────
    public Dictionary<EventCategory, decimal> FullPointsByCategory { get; set; } = new();
    public Dictionary<EventCategory, decimal> SplitPointsByCategory { get; set; } = new();

    // ── 4-event limit ─────────────────────────────────────────
    public int EventCount { get; set; }
    public bool ExceedsEventLimit => EventCount > 4;
}
