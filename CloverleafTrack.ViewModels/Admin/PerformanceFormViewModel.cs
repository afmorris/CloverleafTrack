using System.ComponentModel.DataAnnotations;
using CloverleafTrack.Models;
using CloverleafTrack.Models.Enums;

namespace CloverleafTrack.ViewModels.Admin;

public class PerformanceFormViewModel
{
    public int Id { get; set; }

    // Context (persisted across entries)
    [Required(ErrorMessage = "Meet is required")]
    public int MeetId { get; set; }
    public string? MeetName { get; set; }
    public DateTime? MeetDate { get; set; }
    public string? LocationName { get; set; }

    [Required(ErrorMessage = "Event is required")]
    public int EventId { get; set; }
    public string? EventName { get; set; }
    public short? EventGender { get; set; }
    public EventType? EventType { get; set; }
    public int? EventAthleteCount { get; set; }

    // For individual performances
    public int? AthleteId { get; set; }

    // For relay performances
    public List<int> RelayAthleteIds { get; set; } = new();

    // Performance data
    public double? TimeSeconds { get; set; }
    public double? DistanceInches { get; set; }

    [StringLength(10, ErrorMessage = "Time must be less than 10 characters")]
    public string? TimeInput { get; set; }

    [StringLength(10, ErrorMessage = "Distance must be less than 10 characters")]
    public string? DistanceInput { get; set; }

    // Flags (calculated)
    public bool SchoolRecord { get; set; }
    public bool SeasonBest { get; set; }
    public bool PersonalBest { get; set; }

    // Dropdowns and selections
    public List<Meet> AvailableMeets { get; set; } = new();
    public List<Event> AvailableEvents { get; set; } = new();
    public List<Athlete> EligibleAthletes { get; set; } = new();

    // Duplicate detection
    public Performance? PossibleDuplicate { get; set; }

    // PR Preview
    public Performance? CurrentPR { get; set; }
    public string? ImprovementText { get; set; }

    // Recent relay team suggestion
    public List<Athlete>? RecentRelayTeam { get; set; }

    // Last entry info
    public PerformanceLastEntryViewModel? LastEntry { get; set; }

    // Meet performance count
    public int PerformanceCountForMeet { get; set; }
}

public class PerformanceLastEntryViewModel
{
    public string AthleteName { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public string PerformanceValue { get; set; } = string.Empty;
    public bool WasPR { get; set; }
}