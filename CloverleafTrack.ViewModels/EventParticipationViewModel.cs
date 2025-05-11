using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.ViewModels;

public class EventParticipationViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PersonalRecord { get; set; } = string.Empty;
    public Environment Environment { get; set; }
    public int SortOrder { get; set; }
}