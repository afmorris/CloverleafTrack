using CloverleafTrack.Models.Enums;

namespace CloverleafTrack.Models;

/// <summary>AI-generated career recap of a single athlete. One row per athlete (replaced on regenerate).</summary>
public class AthleteSummary
{
    public int Id { get; set; }
    public int AthleteId { get; set; }
    public string SummaryText { get; set; } = string.Empty;

    /// <summary>SHA-256 of the fact sheet + prompt version the text was generated from.</summary>
    public string InputHash { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public SummaryStatus Status { get; set; } = SummaryStatus.Draft;
    public DateTime GeneratedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
}
