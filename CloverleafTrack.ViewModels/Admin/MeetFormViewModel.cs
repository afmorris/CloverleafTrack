using System.ComponentModel.DataAnnotations;
using CloverleafTrack.Models.Enums;
using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.ViewModels.Admin;

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

    [StringLength(1000)]
    [Display(Name = "Entry Notes")]
    public string? EntryNotes { get; set; }

    // For dropdowns
    public List<LocationOptionViewModel> Locations { get; set; } = new();
    public List<SeasonOptionViewModel> Seasons { get; set; } = new();
    public List<LocationOptionViewModel> RecentLocations { get; set; } = new();
}