using CloverleafTrack.Models.Enums;

namespace CloverleafTrack.ViewModels.Meets;

public class MeetEventGroupViewModel
{
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public EventCategory EventCategory { get; set; }

    /// <summary>True when higher RawValue is better (jumps/throws, including relay variants). Drives sortable-table data-sort-dir.</summary>
    public bool IsFieldEvent { get; set; }
    public List<MeetPerformanceViewModel> Performances { get; set; } = new();
}