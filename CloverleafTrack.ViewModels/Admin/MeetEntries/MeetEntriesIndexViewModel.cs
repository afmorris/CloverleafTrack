using CloverleafTrack.Models;
using CloverleafTrack.Models.Enums;

namespace CloverleafTrack.ViewModels.Admin.MeetEntries;

public class MeetEntriesIndexViewModel
{
    public int MeetId { get; set; }
    public string MeetName { get; set; } = string.Empty;
    public string MeetSlug { get; set; } = string.Empty;
    public DateTime MeetDate { get; set; }
    public MeetType MeetType { get; set; }

    /// <summary>Opponent schools (for dual/double dual labelling).</summary>
    public List<MeetParticipant> Participants { get; set; } = new();

    /// <summary>All entries grouped by event category then event name.</summary>
    public List<MeetEntryEventGroupViewModel> BoysGroups { get; set; } = new();
    public List<MeetEntryEventGroupViewModel> GirlsGroups { get; set; } = new();
    public List<MeetEntryEventGroupViewModel> MixedGroups { get; set; } = new();
}

public class MeetEntryEventGroupViewModel
{
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public EventCategory EventCategory { get; set; }
    public EventType EventType { get; set; }
    public List<MeetEntryRowViewModel> Entries { get; set; } = new();
}

public class MeetEntryRowViewModel
{
    public int EntryId { get; set; }
    public string AthleteDisplay { get; set; } = string.Empty;
    public bool IsRelay { get; set; }
    public bool HasResult { get; set; }
    public string? FormattedResult { get; set; }
    public bool HasPlacing { get; set; }
    public List<EntryPlacingDisplayViewModel> Placings { get; set; } = new();

    /// <summary>True if the athlete is in more than 4 events at this meet.</summary>
    public bool ExceedsEventLimit { get; set; }
}

public class EntryPlacingDisplayViewModel
{
    public int Place { get; set; }
    public string? OpponentSchoolName { get; set; }

    public string MedalEmoji => Place switch
    {
        1 => "🥇",
        2 => "🥈",
        3 => "🥉",
        _ => $"{Place}th"
    };
}
