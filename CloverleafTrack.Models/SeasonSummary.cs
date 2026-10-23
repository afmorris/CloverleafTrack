using CloverleafTrack.Models.Enums;

namespace CloverleafTrack.Models;

/// <summary>Written "season in words" shown on the season wrap. One row per season (replaced on regenerate). Only Approved text is public.</summary>
public class SeasonSummary
{
    public int Id { get; set; }
    public int SeasonId { get; set; }
    public string SummaryText { get; set; } = string.Empty;

    /// <summary>SHA-256 of the fact sheet + prompt version the text was written from.</summary>
    public string InputHash { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public SummaryStatus Status { get; set; } = SummaryStatus.Draft;
    public DateTime GeneratedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
}
