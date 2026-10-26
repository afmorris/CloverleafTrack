using CloverleafTrack.Models.Enums;

namespace CloverleafTrack.ViewModels.Athletes;

/// <summary>Everything the athlete career wrap page and its poster PDF show, derived from <see cref="AthleteDetailsViewModel"/>. BRAIN.md [C126].</summary>
public class AthleteWrapViewModel
{
    public int AthleteId { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int GraduationYear { get; set; }
    public Gender Gender { get; set; }
    public string Class { get; set; } = string.Empty;
    public string TeamLabel => Gender == Gender.Male ? "Boys" : "Girls";

    public bool HasData => Results > 0;

    /// <summary>"2024-2025" to "2025-2026" style span; the same name twice for a one-season career.</summary>
    public string FirstSeason { get; set; } = string.Empty;
    public string LastSeason { get; set; } = string.Empty;
    public int Seasons { get; set; }
    public int Meets { get; set; }
    /// <summary>Marks only (no NM / NH / DNF / DQ rows).</summary>
    public int Results { get; set; }
    /// <summary>Genuine improvements: a mark that beat an earlier mark by the athlete. A first-ever mark in an event is not counted here.</summary>
    public int PRs { get; set; }
    /// <summary>Events in which the athlete holds the school record today (all-time rank 1; relays count).</summary>
    public int SchoolRecords { get; set; }
    /// <summary>Events in which the athlete's best mark is in the all-time top 10.</summary>
    public int TopTenEvents { get; set; }

    /// <summary>Seasons that predate full results (scoring-only data), so earlier marks and PR counts can be incomplete.</summary>
    public bool HasPartialSeasons { get; set; }

    public string? SummaryText { get; set; }
    public string? SummaryHtml { get; set; }

    public List<AthleteWrapImprovement> Improvements { get; set; } = new();
    public List<AthleteWrapBest> Bests { get; set; } = new();
    public List<AthleteWrapSeason> SeasonRows { get; set; } = new();
    public List<CareerChartViewModel> CareerCharts { get; set; } = new();

    public string Url => $"/roster/{Slug}/wrap";
    public string PdfUrl => $"/roster/{Slug}/wrap.pdf";
}

/// <summary>First recorded mark to best mark in one individual event.</summary>
public class AthleteWrapImprovement
{
    public string EventLabel { get; set; } = string.Empty;
    public string First { get; set; } = string.Empty;
    public string Best { get; set; } = string.Empty;
    /// <summary>"-3.77s" style change, as the career chart formats it.</summary>
    public string Change { get; set; } = string.Empty;
    /// <summary>Relative improvement used only to rank events against each other (seconds and inches are not comparable).</summary>
    public double Percent { get; set; }
    public int Performances { get; set; }
}

public class AthleteWrapBest
{
    public string EventLabel { get; set; } = string.Empty;
    public string Mark { get; set; } = string.Empty;
    public string MeetName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public byte? Percentile { get; set; }
    public int? AllTimeRank { get; set; }
    public bool IsSchoolRecord { get; set; }
    public bool IsRelay { get; set; }
}

public class AthleteWrapSeason
{
    public string Name { get; set; } = string.Empty;
    public string? Class { get; set; }
    public int Events { get; set; }
    public int Results { get; set; }
    public int PRs { get; set; }
    public int SchoolRecords { get; set; }
}
