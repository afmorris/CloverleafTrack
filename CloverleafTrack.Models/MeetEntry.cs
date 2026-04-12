namespace CloverleafTrack.Models;

public class MeetEntry
{
    public int Id { get; set; }
    public int MeetId { get; set; }
    public int EventId { get; set; }

    /// <summary>Null for relay entries; set for individual entries.</summary>
    public int? AthleteId { get; set; }

    /// <summary>Set after the meet when the performance result is entered.</summary>
    public int? PerformanceId { get; set; }

    public Meet Meet { get; set; } = new();
    public Event Event { get; set; } = new();
    public Athlete? Athlete { get; set; }
    public Performance? Performance { get; set; }

    /// <summary>Athletes on a relay entry. Populated for relay entries (AthleteId == null).</summary>
    public List<MeetEntryAthlete> RelayAthletes { get; set; } = new();
}
