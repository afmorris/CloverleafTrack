using CloverleafTrack.Services.Interfaces;
using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using CloverleafTrack.Models.Enums;
using CloverleafTrack.ViewModels;
using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.Services;

public class SeasonService(ISeasonRepository seasonRepository) : ISeasonService
{
    public async Task<int> GetCurrentSeasonAsync()
    {
        var allSeasons = await seasonRepository.GetAllAsync();
        var currentSeason = allSeasons.FirstOrDefault(s => s.IsCurrentSeason);

        if (currentSeason == null)
        {
            throw new InvalidOperationException("No current season found.");
        }

        return currentSeason.EndDate.Year;
    }

    public async Task<List<SeasonCardViewModel>> GetSeasonCardsAsync()
    {
        var seasons = await seasonRepository.GetSeasonsWithMeetsAsync();

        var viewModels = seasons.Select(season => new SeasonCardViewModel
        {
            Id = season.Id,
            Name = season.Name,
            Status = season.Status,
            Notes = season.Notes,
            TotalMeets = season.Meets.Count,
            MeetsEntered = season.Meets.Count(m => m.EntryStatus == MeetEntryStatus.Entered),
            IndoorSchoolRecords = season.Meets
                .Where(m => m.Environment == Environment.Indoor)
                .SelectMany(m => m.Performances)
                .Where(p => p.SchoolRecord)
                .Select(p => new SchoolRecordViewModel
                {
                    EventName = p.Event.Name,
                    RecordHolder = p.Athlete?.FirstName + " " + p.Athlete?.LastName,
                    Performance = FormatPerformance(p),
                    Gender = p.Event.Gender ?? Gender.Male,
                })
                .Distinct()
                .ToList(),
            OutdoorSchoolRecords = season.Meets
                .Where(m => m.Environment == Environment.Outdoor)
                .SelectMany(m => m.Performances)
                .Where(p => p.SchoolRecord)
                .Select(p => new SchoolRecordViewModel
                {
                    EventName = p.Event.Name,
                    RecordHolder = p.Athlete?.FirstName + " " + p.Athlete?.LastName,
                    Performance = FormatPerformance(p),
                    Gender = p.Event.Gender ?? Gender.Male,
                })
                .Distinct()
                .ToList(),
            Meets = season.Meets.OrderBy(m => m.Date).Select(m => new MeetSummaryViewModel
            {
                Id = m.Id,
                Name = m.Name,
                Date = m.Date,
                EntryStatus = m.EntryStatus
            }).ToList()
        }).ToList();

        return viewModels;
    }
    
    private string FormatPerformance(Performance p)
    {
        if (p.Event.EventType is EventType.Field or EventType.FieldRelay)
        {
            return p.DistanceInches.HasValue
                ? FormatDistance(p.DistanceInches.Value)
                : "N/A";
        }

        return p.TimeSeconds.HasValue
            ? FormatTime(p.TimeSeconds.Value)
            : "N/A";
    }

    private static string FormatDistance(double inches)
    {
        var feet = Math.Floor(inches / 12);
        var remaining = inches % 12;
        return $"{feet:0}' {remaining:0.##}\"";
    }

    private static string FormatTime(double seconds)
    {
        var timeSpan = TimeSpan.FromSeconds(seconds);
        return timeSpan.TotalMinutes >= 1
            ? timeSpan.ToString(@"m\:ss\.ff")
            : timeSpan.ToString(@"s\.ff");
    }
}