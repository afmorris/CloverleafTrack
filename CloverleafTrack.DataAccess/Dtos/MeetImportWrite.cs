using CloverleafTrack.Models.Enums;

namespace CloverleafTrack.DataAccess.Dtos;

/// <summary>One thing the admin meet-results import (BRAIN.md [C122]) should write: a result (performance + placings), or only a pre-meet entry.</summary>
public class MeetImportWrite
{
    public int EventId { get; set; }
    public bool IsRelay { get; set; }
    /// <summary>Null for relays.</summary>
    public int? AthleteId { get; set; }
    public List<int> RelayAthleteIds { get; set; } = new();

    /// <summary>False = pre-meet entry only (no performance).</summary>
    public bool CreatesResult { get; set; } = true;
    public ResultStatus Status { get; set; }
    public double? TimeSeconds { get; set; }
    public double? DistanceInches { get; set; }

    /// <summary>A performance for this athlete(s)+event at this meet already exists; reuse it instead of inserting.</summary>
    public int? ExistingPerformanceId { get; set; }
    /// <summary>With <see cref="ExistingPerformanceId"/>: overwrite its mark/status with the values above.</summary>
    public bool ReplaceMark { get; set; }

    public List<MeetImportPlacingWrite> Placings { get; set; } = new();
}

public class MeetImportPlacingWrite
{
    /// <summary>Null = overall placing (invitationals).</summary>
    public int? MeetParticipantId { get; set; }
    public int Place { get; set; }
    public decimal FullPoints { get; set; }
    public decimal SplitPoints { get; set; }
}

public class MeetImportOutcome
{
    public int PerformancesCreated { get; set; }
    public int MarksReplaced { get; set; }
    public int EntriesCreated { get; set; }
    public int EntriesLinked { get; set; }
    public int PlacingsWritten { get; set; }
}
