using CloverleafTrack.Models.Enums;

namespace CloverleafTrack.ViewModels.Admin.Performances;

public class EventOptionViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public EventType EventType { get; set; }
    public EventCategory? EventCategory { get; set; }
    public Gender? Gender { get; set; }
    public int AthleteCount { get; set; }
    public string DisplayText => $"{Name} ({Gender})";
    
    public string CategoryName => EventCategory?.ToString() ?? "Other";
}
