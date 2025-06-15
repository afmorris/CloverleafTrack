using CloverleafTrack.Services.Interfaces;
using CloverleafTrack.DataAccess.Interfaces;
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
                    RecordHolder = "test",//p.Athlete.FirstName + " " + p.Athlete.LastName,
                    Performance = p.DistanceInches.HasValue 
                        ? $"{Math.Floor(p.DistanceInches.Value / 12):0}' {p.DistanceInches.Value % 12:0.##}\""
                        : $"{p.TimeSeconds.Value:0.00}s"
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
                    RecordHolder = "test",//p.Athlete.FirstName + " " + p.Athlete.LastName,
                    Performance = p.DistanceInches.HasValue 
                        ? $"{Math.Floor(p.DistanceInches.Value / 12):0}' {p.DistanceInches.Value % 12:0.##}\""
                        : $"{p.TimeSeconds.Value:0.00}s"
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
}