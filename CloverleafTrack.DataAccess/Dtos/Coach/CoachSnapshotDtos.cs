namespace CloverleafTrack.DataAccess.Dtos.Coach;

// Rows of the nightly snapshot pushed to the coaching subsite (coach.cloverleaftrack.com). BRAIN.md [C133].

public class CoachAthleteDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public short Gender { get; set; }
    public int GraduationYear { get; set; }
}

public class CoachEventDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string EventKey { get; set; } = string.Empty;
    public short EventType { get; set; }
    public short Gender { get; set; }
    public short Environment { get; set; }
    public short EventCategory { get; set; }
    public int SortOrder { get; set; }
}

public class CoachSeasonDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsCurrentSeason { get; set; }
}

/// <summary>An athlete's best mark in one individual event in one season, with the meet where it happened.</summary>
public class CoachSeasonBestDto
{
    public int AthleteId { get; set; }
    public int EventId { get; set; }
    public int SeasonId { get; set; }
    public double? TimeSeconds { get; set; }
    public double? DistanceInches { get; set; }
    public string MeetName { get; set; } = string.Empty;
    public DateTime MeetDate { get; set; }
}

public class CoachMeetDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public short Environment { get; set; }
    public int SeasonId { get; set; }
}

public class CoachSnapshotDto
{
    public List<CoachAthleteDto> Athletes { get; set; } = [];
    public List<CoachEventDto> Events { get; set; } = [];
    public List<CoachSeasonDto> Seasons { get; set; } = [];
    public List<CoachSeasonBestDto> SeasonBests { get; set; } = [];
    public List<CoachMeetDto> Meets { get; set; } = [];
}
