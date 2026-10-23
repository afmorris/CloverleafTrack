using CloverleafTrack.Models.Enums;

namespace CloverleafTrack.ViewModels.Admin.SeasonSummaries;

public class SeasonSummariesIndexViewModel
{
    public List<SeasonSummaryRow> Rows { get; set; } = new();
}

public class SeasonSummaryRow
{
    public int SeasonId { get; set; }
    public string SeasonName { get; set; } = string.Empty;
    public string WrapUrl { get; set; } = string.Empty;
    public int? SummaryId { get; set; }
    public string? SummaryText { get; set; }
    public SummaryStatus? Status { get; set; }
    public string? Model { get; set; }
    public DateTime? GeneratedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
}
