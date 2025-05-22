using CloverleafTrack.Models.Enums;

namespace CloverleafTrack.ViewModels;

public class MeetSummaryViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public MeetEntryStatus EntryStatus { get; set; }
}