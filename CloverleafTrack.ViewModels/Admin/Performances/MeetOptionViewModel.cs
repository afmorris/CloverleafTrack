using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.ViewModels.Admin.Performances;

public class MeetOptionViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public Environment Environment { get; set; }
    public string SeasonName { get; set; } = string.Empty;
    
    public string DisplayText => $"{Name} - {Date:MMM d, yyyy} ({Environment})";
}
