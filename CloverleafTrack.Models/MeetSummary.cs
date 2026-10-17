using CloverleafTrack.Models.Enums;

namespace CloverleafTrack.Models;

/// <summary>AI-generated recap of a single meet. One row per meet (replaced on regenerate).</summary>
public class MeetSummary
{
    public int Id { get; set; }
    public int MeetId { get; set; }
    public string SummaryText { get; set; } = string.Empty;

    /// <summary>SHA-256 of the fact sheet + prompt version the text was generated from. Unchanged hash = nothing to regenerate.</summary>
    public string InputHash { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public SummaryStatus Status { get; set; } = SummaryStatus.Draft;
    public DateTime GeneratedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
}
