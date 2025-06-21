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
    
    public List<Performance> Performances { get; set; } = new();
    public Location Location { get; set; }
    public Season Season { get; set; }
    
    public int PRCount { get; set; }
    public int SchoolRecordCount { get; set; }

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