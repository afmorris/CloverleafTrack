namespace CloverleafTrack.Models;

public class MeetEventScoringOverride
{
    public int Id { get; set; }
    public int MeetId { get; set; }
    public int EventId { get; set; }
    public int ScoringTemplateId { get; set; }

    public Event Event { get; set; } = new();
    public ScoringTemplate ScoringTemplate { get; set; } = new();
}
