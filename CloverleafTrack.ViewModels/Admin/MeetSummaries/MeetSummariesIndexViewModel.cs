using CloverleafTrack.Models.Enums;

namespace CloverleafTrack.ViewModels.Admin.MeetSummaries;

public class MeetSummariesIndexViewModel
{
    public int SeasonId { get; set; }
    public string SeasonName { get; set; } = string.Empty;
    public List<SeasonChoice> Seasons { get; set; } = new();
    public List<MeetSummaryRow> Rows { get; set; } = new();

    public int WithSummary => Rows.Count(r => r.SummaryId.HasValue);
    public int Approved => Rows.Count(r => r.Status == SummaryStatus.Approved);
}

public class SeasonChoice
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class MeetSummaryRow
{
    public int MeetId { get; set; }
    public string MeetName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string PublicUrl { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public bool IsJv { get; set; }

    public int? SummaryId { get; set; }
    public string? SummaryText { get; set; }

    /// <summary>HTML-encoded SummaryText with athlete and event names linked. Safe to render with Html.Raw.</summary>
    public string? SummaryHtml { get; set; }
    public SummaryStatus? Status { get; set; }
    public string? Model { get; set; }
    public DateTime? GeneratedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
}
