using CloverleafTrack.DataAccess.Dtos;
using CloverleafTrack.Models;
using CloverleafTrack.Models.Enums;

namespace CloverleafTrack.Web.Import;

/// <summary>Everything the planner needs to know about the meet, loaded once by <see cref="MeetImportService"/> so planning itself needs no database.</summary>
public class MeetImportContext
{
    public Meet Meet { get; set; } = new();
    /// <summary>Events for the meet's environment (outdoor meets only see outdoor events).</summary>
    public List<Event> Events { get; set; } = new();
    public List<Athlete> Athletes { get; set; } = new();
    public List<MeetParticipant> Participants { get; set; } = new();
    public List<MeetEntryDto> Entries { get; set; } = new();
    public List<ExistingPerformance> Performances { get; set; } = new();
    public List<MeetPlacing> Placings { get; set; } = new();
}

public class ExistingPerformance
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public int? AthleteId { get; set; }
    public List<int> RelayAthleteIds { get; set; } = new();
    public ResultStatus Status { get; set; }
    public double? TimeSeconds { get; set; }
    public double? DistanceInches { get; set; }
}
