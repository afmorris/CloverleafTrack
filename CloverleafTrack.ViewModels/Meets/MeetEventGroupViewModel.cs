using CloverleafTrack.Models.Enums;

namespace CloverleafTrack.ViewModels.Meets;

public class MeetEventGroupViewModel
{
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public EventCategory EventCategory { get; set; }
    public List<MeetPerformanceViewModel> Performances { get; set; } = new();
}