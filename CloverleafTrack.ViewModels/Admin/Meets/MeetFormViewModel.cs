using System.ComponentModel.DataAnnotations;
using CloverleafTrack.Models;
using CloverleafTrack.Models.Enums;
using CloverleafTrack.ViewModels.Admin.ScoringTemplates;
using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.ViewModels.Admin.Meets;

public class MeetFormViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    [Display(Name = "Meet Name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Date")]
    public DateTime Date { get; set; } = DateTime.Today;

    [Required]
    [Display(Name = "Location")]
    public int LocationId { get; set; }

    [Required]
    [Display(Name = "Environment")]
    public Environment Environment { get; set; }

    [Display(Name = "Hand Timed")]
    public bool HandTimed { get; set; }

    [Required]
    [Display(Name = "Season")]
    public int SeasonId { get; set; }

    [Display(Name = "Entry Status")]
    public MeetEntryStatus EntryStatus { get; set; }

    [Display(Name = "Meet Type")]
    public MeetType MeetType { get; set; } = MeetType.Dual;

    [Display(Name = "Scoring Template")]
    public int? ScoringTemplateId { get; set; }

    [StringLength(1000)]
    [Display(Name = "Entry Notes")]
    public string? EntryNotes { get; set; }

    // Dropdowns
    public List<LocationOptionViewModel> Locations { get; set; } = new();
    public List<SeasonOptionViewModel> Seasons { get; set; } = new();
    public List<LocationOptionViewModel> RecentLocations { get; set; } = new();
    public List<ScoringTemplateOptionViewModel> ScoringTemplates { get; set; } = new();
    public List<SchoolOptionViewModel> Schools { get; set; } = new();

    /// <summary>Selected school IDs for each participant slot.</summary>
    public List<int> ParticipantSchoolIds { get; set; } = new();

    /// <summary>Existing MeetParticipant.Id values — index-aligned with ParticipantSchoolIds for update vs. insert.</summary>
    public List<int> ParticipantIds { get; set; } = new();

    /// <summary>Loaded on Edit to display current participants.</summary>
    public List<MeetParticipant> ExistingParticipants { get; set; } = new();

    /// <summary>Team result rows for the Edit form — server-rendered based on meet type and participants.</summary>
    public List<MeetTeamResultFormViewModel> TeamResults { get; set; } = new();
}
