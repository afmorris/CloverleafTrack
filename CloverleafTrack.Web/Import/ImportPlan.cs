using CloverleafTrack.Models.Enums;

namespace CloverleafTrack.Web.Import;

public enum ImportRowKind { Result, Entry }

public enum ImportRowState
{
    /// <summary>Will be created.</summary>
    New,
    /// <summary>The mark is already stored; only missing placings would be added.</summary>
    AddsPlacings,
    /// <summary>Everything in the file is already stored.</summary>
    AlreadyThere,
    /// <summary>A different mark (or place) is stored. Applying replaces it, so it is off by default.</summary>
    Differs,
    /// <summary>Cannot be applied until the file is fixed (or an athlete is chosen).</summary>
    Problem
}

public class AthleteCandidate
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int GraduationYear { get; set; }
}

public class PlanPlacing
{
    public int? MeetParticipantId { get; set; }
    public string OpponentLabel { get; set; } = string.Empty;
    public int Place { get; set; }
}

public class ImportRow
{
    public int Index { get; set; }
    public ImportRowKind Kind { get; set; }
    public ImportRowState State { get; set; }

    public Gender? Gender { get; set; }
    public string EventLabel { get; set; } = string.Empty;
    public int? EventId { get; set; }
    public bool IsRelay { get; set; }
    public EventType? EventType { get; set; }
    public int AthleteCount { get; set; }

    /// <summary>The input as written in the file (shown when something could not be resolved).</summary>
    public string AthleteText { get; set; } = string.Empty;
    public int? AthleteId { get; set; }
    public List<int> RelayAthleteIds { get; set; } = new();
    public string AthleteDisplay { get; set; } = string.Empty;
    /// <summary>Individual rows only: possible athletes when the name was not an exact, unique match.</summary>
    public List<AthleteCandidate> Candidates { get; set; } = new();

    public ResultStatus Status { get; set; }
    public double? TimeSeconds { get; set; }
    public double? DistanceInches { get; set; }
    public string MarkText { get; set; } = string.Empty;
    public string? ExistingMarkText { get; set; }
    public int? ExistingPerformanceId { get; set; }

    public List<PlanPlacing> Placings { get; set; } = new();
    public bool MatchesEntry { get; set; }
    public string? Note { get; set; }

    public List<string> Problems { get; set; } = new();
    public List<string> Warnings { get; set; } = new();

    public bool CanApply => State != ImportRowState.Problem && State != ImportRowState.AlreadyThere;
    /// <summary>Pre-ticked in the review grid. Differs rows replace stored data, so they start unticked.</summary>
    public bool DefaultInclude => State is ImportRowState.New or ImportRowState.AddsPlacings;
}

public class ImportPlan
{
    public List<ImportRow> Rows { get; set; } = new();
    public List<string> FileProblems { get; set; } = new();
    public string? Source { get; set; }
    public string? Notes { get; set; }

    public int Count(ImportRowState state) => Rows.Count(r => r.State == state);
}
