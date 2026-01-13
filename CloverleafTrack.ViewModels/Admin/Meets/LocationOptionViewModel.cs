namespace CloverleafTrack.ViewModels.Admin.Meets;

public class LocationOptionViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    
    public string DisplayText => !string.IsNullOrEmpty(City) && !string.IsNullOrEmpty(State)
        ? $"{Name} ({City}, {State})"
        : Name;
}
