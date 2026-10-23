using CloverleafTrack.Models.Enums;
using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.ViewModels.Seasons;

/// <summary>Everything the season wrap page and its poster PDF show, computed from the season's data on every request. BRAIN.md [C123].</summary>
public class SeasonWrapViewModel
{
    public int SeasonId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    /// <summary>The season is still running, so every number is "so far".</summary>
    public bool InProgress { get; set; }
    /// <summary>Date of the last meet with results.</summary>
    public DateTime? ThroughDate { get; set; }
    public bool HasData => Meets > 0;

    // Headline numbers
    public int Meets { get; set; }
    public int Results { get; set; }
    public int AthletesCompeted { get; set; }
    public int PRs { get; set; }
    public int AthletesWithPRs { get; set; }
    /// <summary>Events in which a school record was set this season (one per event, however often it was broken).</summary>
    public int SchoolRecordsSet { get; set; }
    /// <summary>Marks that broke the school record when run; larger than <see cref="SchoolRecordsSet"/> when a record fell more than once.</summary>
    public int SchoolRecordBreaks { get; set; }
    public int TopTenMarks { get; set; }

    public List<WrapSchoolRecord> SchoolRecords { get; set; } = new();
    public List<WrapMeetScore> BestMeets { get; set; } = new();
    public List<WrapStandout> Standouts { get; set; } = new();
    public List<WrapBreakout> Breakouts { get; set; } = new();
    public List<WrapImprovement> Improvements { get; set; } = new();
    public List<WrapSenior> Seniors { get; set; } = new();
    public int SeniorClassYear { get; set; }

    public WrapDualRecord? DualRecord { get; set; }

    public bool ScoringEnabled { get; set; }
    public List<WrapScoringLeader> BoysLeaders { get; set; } = new();
    public List<WrapScoringLeader> GirlsLeaders { get; set; } = new();

    public List<WrapMeetPoint> Progression { get; set; } = new();

    /// <summary>Approved written summary of the season, when one exists (BRAIN.md [C123]); null otherwise.</summary>
    public string? SummaryText { get; set; }

    public string Url => $"/seasons/{Name}/wrap";
    public string PdfUrl => $"/seasons/{Name}/wrap.pdf";
}

public class WrapPerson
{
    public string Name { get; set; } = string.Empty;
    /// <summary>Athlete page slug.</summary>
    public string Slug { get; set; } = string.Empty;
}

public class WrapSchoolRecord
{
    public string EventLabel { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public Environment Environment { get; set; }
    /// <summary>One person for an individual event, the team for a relay.</summary>
    public List<WrapPerson> Holders { get; set; } = new();
    public string Mark { get; set; } = string.Empty;
    public string MeetName { get; set; } = string.Empty;
    public string MeetSlug { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    /// <summary>Times the record was broken during the season (1 = once).</summary>
    public int TimesBroken { get; set; }
    /// <summary>The mark is still the school record today.</summary>
    public bool StillRecord { get; set; }
}

public class WrapMeetScore
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public double Score { get; set; }
    public int PRs { get; set; }
}

public class WrapStandout
{
    public WrapPerson Athlete { get; set; } = new();
    public string EventLabel { get; set; } = string.Empty;
    public string Mark { get; set; } = string.Empty;
    public int Percentile { get; set; }
    public int? AllTimeRank { get; set; }
    public string MeetName { get; set; } = string.Empty;
    public string MeetSlug { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}

public class WrapBreakout
{
    public WrapPerson Athlete { get; set; } = new();
    /// <summary>Marks that beat an earlier mark by this athlete (first-ever marks are not counted here).</summary>
    public int PRs { get; set; }
    public int Events { get; set; }
    /// <summary>"100M: 12.80 → 12.41".</summary>
    public string Highlight { get; set; } = string.Empty;
}

public class WrapImprovement
{
    public WrapPerson Athlete { get; set; } = new();
    public string EventLabel { get; set; } = string.Empty;
    public string Before { get; set; } = string.Empty;
    public string After { get; set; } = string.Empty;
    /// <summary>"0.43s faster" / "2' 3\" farther".</summary>
    public string Change { get; set; } = string.Empty;
    public double Percent { get; set; }
}

public class WrapSenior
{
    public WrapPerson Athlete { get; set; } = new();
    public int Events { get; set; }
    public int PRs { get; set; }
    public string BestEventLabel { get; set; } = string.Empty;
    public string BestMark { get; set; } = string.Empty;
    public int BestPercentile { get; set; }
}

public class WrapDualRecord
{
    public WrapRecord Boys { get; set; } = new();
    public WrapRecord Girls { get; set; } = new();
    public List<WrapDualRow> Duals { get; set; } = new();
}

public class WrapRecord
{
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Ties { get; set; }
    public string Text => Ties > 0 ? $"{Wins}-{Losses}-{Ties}" : $"{Wins}-{Losses}";
    public bool Any => Wins + Losses + Ties > 0;
}

public class WrapDualRow
{
    public DateTime Date { get; set; }
    public string MeetName { get; set; } = string.Empty;
    public string MeetSlug { get; set; } = string.Empty;
    public string Opponent { get; set; } = string.Empty;
    /// <summary>"W 85-52", "L 48-90", "T 70-70"; empty when there was no score for that gender.</summary>
    public string Boys { get; set; } = string.Empty;
    public string Girls { get; set; } = string.Empty;
}

public class WrapScoringLeader
{
    public WrapPerson Athlete { get; set; } = new();
    public decimal Points { get; set; }
    public int Events { get; set; }
}

public class WrapMeetPoint
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int Results { get; set; }
    public int PRs { get; set; }
    public int SchoolRecords { get; set; }
    /// <summary>Mean percentile of the meet's marks (0 when none have one).</summary>
    public double AveragePercentile { get; set; }
}
