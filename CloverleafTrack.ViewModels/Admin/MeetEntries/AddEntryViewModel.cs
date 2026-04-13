using System.ComponentModel.DataAnnotations;
using CloverleafTrack.Models.Enums;

namespace CloverleafTrack.ViewModels.Admin.MeetEntries;

public class AddEntryViewModel
{
    public int MeetId { get; set; }
    public string MeetName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Event")]
    public int EventId { get; set; }

    /// <summary>Null for relay entries.</summary>
    [Display(Name = "Athlete")]
    public int? AthleteId { get; set; }

    public EventType EventType { get; set; }
    public int EventAthleteCount { get; set; }

    /// <summary>Athlete IDs for relay entries, indexed 0..3.</summary>
    public List<int?> RelayAthleteIds { get; set; } = new(new int?[4]);

    // Dropdowns (plain option POCOs — no ASP.NET Core reference needed)
    public List<MeetEntryEventOptionViewModel> Events { get; set; } = new();
    public List<MeetEntryAthleteOptionViewModel> Athletes { get; set; } = new();
}

public class MeetEntryEventOptionViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public EventType EventType { get; set; }
    public int AthleteCount { get; set; }
    public Gender Gender { get; set; }
    public string DisplayText => Gender switch
    {
        Gender.Male   => $"{Name} (Boys)",
        Gender.Female => $"{Name} (Girls)",
        Gender.Mixed  => $"{Name} (Mixed)",
        _             => Name
    };
}

public class MeetEntryAthleteOptionViewModel
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string DisplayText => $"{LastName}, {FirstName}";
}
