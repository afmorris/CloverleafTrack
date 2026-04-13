using System.ComponentModel.DataAnnotations;
using CloverleafTrack.Models;
using CloverleafTrack.Models.Enums;

namespace CloverleafTrack.ViewModels.Admin.MeetEntries;

public class EnterResultViewModel
{
    public int EntryId { get; set; }
    public int MeetId { get; set; }
    public string MeetName { get; set; } = string.Empty;
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public EventType EventType { get; set; }
    public string AthleteDisplay { get; set; } = string.Empty;
    public bool IsRelay { get; set; }
    public MeetType MeetType { get; set; }

    /// <summary>Opponent schools for dual/double dual. Empty for invitationals.</summary>
    public List<MeetParticipant> Participants { get; set; } = new();

    // ── Performance input ─────────────────────────────────────
    /// <summary>Time string (e.g., "1:23.45") for running events.</summary>
    [Display(Name = "Time")]
    public string? TimeInput { get; set; }

    /// <summary>Distance string (e.g., "42' 6.5\"") for field events.</summary>
    [Display(Name = "Distance")]
    public string? DistanceInput { get; set; }

    // ── Placing input ─────────────────────────────────────────
    /// <summary>
    /// One entry per opponent (dual/double dual) or one entry with ParticipantId = null (invitational).
    /// Indexed to match Participants list; for invitational use index 0 with ParticipantId = null.
    /// </summary>
    public List<PlaceInputRow> PlaceInputs { get; set; } = new();

    // ── Computed preview (read-only, populated by JS or on re-render) ─
    public decimal PreviewFullPoints { get; set; }
    public decimal PreviewSplitPoints { get; set; }
}

public class PlaceInputRow
{
    public int? MeetParticipantId { get; set; }
    public string OpponentLabel { get; set; } = string.Empty;

    [Range(1, 999)]
    public int? Place { get; set; }
}
