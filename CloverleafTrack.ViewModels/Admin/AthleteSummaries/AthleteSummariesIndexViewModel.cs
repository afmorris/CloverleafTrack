using CloverleafTrack.Models.Enums;

namespace CloverleafTrack.ViewModels.Admin.AthleteSummaries;

public class AthleteSummariesIndexViewModel
{
    public int GraduationYear { get; set; }
    public List<int> GraduationYears { get; set; } = new();
    public List<AthleteSummaryRow> Rows { get; set; } = new();

    /// <summary>When false (default), athletes who did not compete in their senior school year are left out.</summary>
    public bool IncludeAll { get; set; }

    /// <summary>Athletes in this class with no performance in their senior school year (hidden unless IncludeAll).</summary>
    public int ExcludedCount { get; set; }

    public int WithSummary => Rows.Count(r => r.SummaryId.HasValue);
    public int Approved => Rows.Count(r => r.Status == SummaryStatus.Approved);
}

public class AthleteSummaryRow
{
    public int AthleteId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Team { get; set; } = string.Empty;
    public string PublicUrl { get; set; } = string.Empty;
    public bool CompetedSenior { get; set; } = true;

    public int? SummaryId { get; set; }
    public string? SummaryText { get; set; }

    /// <summary>HTML-encoded SummaryText with athlete, meet and event names linked. Safe to render with Html.Raw.</summary>
    public string? SummaryHtml { get; set; }
    public SummaryStatus? Status { get; set; }
    public string? Model { get; set; }
    public DateTime? GeneratedAt { get; set; }
}
