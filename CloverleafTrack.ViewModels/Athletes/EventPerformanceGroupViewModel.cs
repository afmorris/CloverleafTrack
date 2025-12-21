using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.ViewModels.Athletes;

public class EventPerformanceGroupViewModel
{
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public Environment Environment { get; set; }
    public string PersonalRecordPerformance { get; set; } = string.Empty;
    public DateTime PersonalRecordDate { get; set; }
    public int EventCategorySortOrder { get; set; }
    public List<IndividualPerformanceViewModel> Performances { get; set; } = new();
}