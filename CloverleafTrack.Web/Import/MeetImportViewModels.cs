namespace CloverleafTrack.Web.Import;

public class MeetImportIndexViewModel
{
    public int MeetId { get; set; }
    public string MeetName { get; set; } = string.Empty;
    public DateTime MeetDate { get; set; }
    public string? Error { get; set; }
    public string Json { get; set; } = string.Empty;
}

/// <summary>One row's choices as posted back from the review grid.</summary>
public class MeetImportRowChoice
{
    public int Index { get; set; }
    public bool Include { get; set; }
    /// <summary>Athlete picked from the "did you mean" list; replaces whatever the name matched.</summary>
    public int? AthleteId { get; set; }
}

public class MeetImportReviewViewModel
{
    public int MeetId { get; set; }
    public string MeetName { get; set; } = string.Empty;
    public DateTime MeetDate { get; set; }
    public string Json { get; set; } = string.Empty;
    public ImportPlan Plan { get; set; } = new();
    /// <summary>Row indexes ticked in the grid.</summary>
    public HashSet<int> Included { get; set; } = new();
    public Dictionary<int, int> AthleteOverrides { get; set; } = new();
}
