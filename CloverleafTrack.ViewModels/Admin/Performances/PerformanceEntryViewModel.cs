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

    // NM / NH / DNF / DQ — competed but no mark. Mark (0) is the normal path. When not Mark, the time and distance
    // inputs must be blank; attempts / bars may still be recorded. Inferred as NM / NH when an attempt series is all
    // fouls or a bar ladder has no cleared height. See BRAIN.md [C67].
    [Display(Name = "Result")]
    public ResultStatus ResultStatus { get; set; }
    
    // Relay athletes
    public List<int> RelayAthleteIds { get; set; } = new();

    // Field-event attempt series — optional, additive. Only offered for Field/ThrowsRelay/JumpRelay
    // events (see the isFieldEvent JS/Razor check shared with the single-mark DistanceInput field).
    // Off (false) by default so the existing fast single-mark path is completely unchanged.
    [Display(Name = "Record full attempt series")]
    public bool RecordFullSeries { get; set; }

    public List<PerformanceAttemptInputViewModel> Attempts { get; set; } = Enumerable.Range(1, 6)
        .Select(n => new PerformanceAttemptInputViewModel { AttemptNumber = n })
        .ToList();

    // Running/relay rounds — optional, additive. When at least two are filled in, Time above is
    // overwritten with the best (min) round time. Leave all blank for the normal single-mark path.
    [Display(Name = "Prelim time")]
    public string? PrelimInput { get; set; }

    [Display(Name = "Semifinal time")]
    public string? SemiInput { get; set; }

    [Display(Name = "Final time")]
    public string? FinalInput { get; set; }

    // High jump / pole vault bar ladder — optional, additive. One entry per line (or ';'): height then outcome,
    // e.g. 4'4" XXO. The highest cleared bar overwrites the distance above. Blank = no ladder recorded.
    [Display(Name = "Bars")]
    public string? BarsInput { get; set; }

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
