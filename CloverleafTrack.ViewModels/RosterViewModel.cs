using CloverleafTrack.Models.Enums;

namespace CloverleafTrack.ViewModels;

public class RosterViewModel
{
    public Dictionary<EventCategory, List<AthleteViewModel>> ActiveAthletes { get; set; } = new();
    public Dictionary<int, List<AthleteViewModel>> FormerAthletes { get; set; } = new();
}