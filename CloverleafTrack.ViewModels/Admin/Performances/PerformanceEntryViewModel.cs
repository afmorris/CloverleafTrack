using System.ComponentModel.DataAnnotations;
using CloverleafTrack.Models.Enums;

namespace CloverleafTrack.ViewModels.Admin.Performances;

public class PerformanceEntryViewModel
{
    public int Id { get; set; }
    
    [Required]
    [Display(Name = "Meet")]
    public int MeetId { get; set; }
    
    [Required]
    [Display(Name = "Event")]
    public int EventId { get; set; }
    
    [Display(Name = "Athlete")]
    public int? AthleteId { get; set; }
    
    [Display(Name = "Time (seconds)")]
    public string? TimeInput { get; set; }
    
    [Display(Name = "Distance")]
    public string? DistanceInput { get; set; }
    
    // Relay athletes
    public List<int> RelayAthleteIds { get; set; } = new();
    
    // For display
    public string MeetName { get; set; } = string.Empty;
    public DateTime MeetDate { get; set; }
    public string EventName { get; set; } = string.Empty;
    public EventType EventType { get; set; }
    public int EventAthleteCount { get; set; }
    public Gender? EventGender { get; set; }
    
    // For dropdowns
    public List<MeetOptionViewModel> Meets { get; set; } = new();
    public List<EventOptionViewModel> Events { get; set; } = new();
    public List<AthleteOptionViewModel> Athletes { get; set; } = new();
    
    // Context
    public int PerformanceCount { get; set; }
    public PerformanceSummaryViewModel? LastEntry { get; set; }
    public PerformanceSummaryViewModel? SimilarPerformance { get; set; }
    public PerformanceSummaryViewModel? CurrentPR { get; set; }
    public List<int>? RecentRelayTeam { get; set; }
}
