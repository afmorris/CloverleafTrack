using CloverleafTrack.Models.Enums;
using Slugify;

namespace CloverleafTrack.ViewModels;

public class SchoolRecordViewModel
{
    public string EventName { get; set; } = string.Empty;
    public string RecordHolderFirstName { get; set; } = string.Empty;
    public string RecordHolderLastName { get; set; } = string.Empty;
    public string RecordHolder => $"{RecordHolderFirstName} {RecordHolderLastName}";
    public string RecordHolderSlug
    {
        get
        {
            var helper = new SlugHelper();
            return helper.GenerateSlug($"{RecordHolderFirstName}-{RecordHolderLastName}");
        }
    }
    public string Performance { get; set; } = string.Empty;
    public string MeetName { get; set; } = string.Empty;
    public DateTime MeetDate { get; set; }
    public Gender  Gender { get; set; }
}