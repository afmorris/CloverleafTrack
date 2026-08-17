using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.ViewModels;

public class EventParticipationViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PersonalRecord { get; set; } = string.Empty;
    public Environment Environment { get; set; }
    public int SortOrder { get; set; }

    /// <summary>Percentile (1-100) of the athlete's best performance in this event. Null when not yet populated by sp_RebuildLeaderboards (fewer than the program's minimum mark threshold). Drives the Roster mark tint and percentile bar (issue #23).</summary>
    public byte? Percentile { get; set; }
    public bool IsFieldEvent { get; set; }
}