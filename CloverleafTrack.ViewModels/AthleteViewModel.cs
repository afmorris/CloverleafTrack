using CloverleafTrack.Models.Enums;
using Slugify;
using System.Xml.Linq;

namespace CloverleafTrack.ViewModels;

public class AthleteViewModel
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string AthleteSlug
    {
        get
        {
            var helper = new SlugHelper();
            return helper.GenerateSlug($"{FirstName}-{LastName}");
        }
    }
    public int GraduationYear { get; set; }
    public Gender Gender { get; set; }

    public string Class { get; set; } = string.Empty;
    public List<EventParticipationViewModel> EventsInCategory { get; set; } = new();
}