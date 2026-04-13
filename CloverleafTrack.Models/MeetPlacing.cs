namespace CloverleafTrack.Models;

public class MeetPlacing
{
    public int Id { get; set; }
    public int MeetId { get; set; }
    public int PerformanceId { get; set; }

    /// <summary>
    /// Null for invitationals (single placing in the full field).
    /// Set to a MeetParticipant.Id for dual/double dual (one row per opponent school).
    /// </summary>
    public int? MeetParticipantId { get; set; }

    public int Place { get; set; }

    /// <summary>Points where relay members each receive the full template points value.</summary>
    public decimal FullPoints { get; set; }

    /// <summary>Points where relay points are divided by the number of relay members.</summary>
    public decimal SplitPoints { get; set; }

    public MeetParticipant? MeetParticipant { get; set; }
}
