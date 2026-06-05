using CloverleafTrack.Models.Enums;

namespace CloverleafTrack.ViewModels.Meets;

public class TeamResultViewModel
{
    public MeetType MeetType { get; set; }
    public List<TeamResultEntryViewModel> Entries { get; set; } = new();
    public bool HasResult => Entries.Count > 0;
}

public class TeamResultEntryViewModel
{
    public Gender Gender { get; set; }
    public string? OpponentName { get; set; }
    public decimal? OurScore { get; set; }
    public decimal? OpponentScore { get; set; }
    public int? Place { get; set; }
    public int? FieldSize { get; set; }
    public bool IsDualStyle { get; set; }

    public string GenderLabel => Gender == Gender.Male ? "Boys" : "Girls";

    public bool Won => IsDualStyle && OurScore.HasValue && OpponentScore.HasValue && OurScore > OpponentScore;

    public string Label
    {
        get
        {
            if (IsDualStyle && OurScore.HasValue)
            {
                var vs = OpponentName != null ? $" vs. {OpponentName}" : "";
                return $"{GenderLabel} {(Won ? "W" : "L")} {OurScore:0.##}-{OpponentScore:0.##}{vs}";
            }
            if (!IsDualStyle && Place.HasValue)
            {
                var of = FieldSize.HasValue ? $" of {FieldSize}" : "";
                return $"{GenderLabel} {Place}{Ordinal(Place.Value)}{of}";
            }
            return string.Empty;
        }
    }

    private static string Ordinal(int n) => (n % 100) switch
    {
        11 or 12 or 13 => "th",
        _ => (n % 10) switch { 1 => "st", 2 => "nd", 3 => "rd", _ => "th" }
    };
}
