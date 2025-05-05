namespace CloverleafTrack.ViewModels;

public class RosterViewModel
{
    public List<AthleteViewModel> ActiveAthletes { get; set; } = new();
    public List<AthleteViewModel> GraduatedAthletes { get; set; } = new();
}