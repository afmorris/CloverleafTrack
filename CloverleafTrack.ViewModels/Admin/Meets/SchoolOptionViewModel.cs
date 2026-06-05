namespace CloverleafTrack.ViewModels.Admin.Meets;

public class SchoolOptionViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public string DisplayText => string.IsNullOrEmpty(ShortName) ? Name : $"{Name} ({ShortName})";
}
