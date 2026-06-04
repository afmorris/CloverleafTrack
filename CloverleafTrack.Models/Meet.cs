using System.Web;
using CloverleafTrack.Models.Enums;
using Slugify;
using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.Models;

public class Meet
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public Environment Environment { get; set; }
    public bool HandTimed { get; set; }
    
    public int LocationId { get; set; }
    public int SeasonId { get; set; }
    public MeetEntryStatus EntryStatus { get; set; }
    public MeetType MeetType { get; set; }
    public int? ScoringTemplateId { get; set; }

    public List<Performance> Performances { get; set; } = new();
    public Location Location { get; set; }
    public Season Season { get; set; }
    public List<MeetParticipant> Participants { get; set; } = new();
    public ScoringTemplate? ScoringTemplate { get; set; }

    public int PRCount { get; set; }
    public int SchoolRecordCount { get; set; }
    public string? EntryNotes { get; set; } = string.Empty;

    // Optional team score / placement (entered post-meet)
    public int? BoysScore { get; set; }
    public int? BoysOpponentScore { get; set; }
    public int? GirlsScore { get; set; }
    public int? GirlsOpponentScore { get; set; }
    public int? BoysPlace { get; set; }
    public int? GirlsPlace { get; set; }
    public int? FieldSize { get; set; }

    public string Slug
    {
        get
        {
            var helper = new SlugHelper();
            return helper.GenerateSlug(Name);
        }
    }

    public string ResultsUrl => $"/meets/{Slug}";
}