using CloverleafTrack.Services.Interfaces;
using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models.Enums;
using CloverleafTrack.ViewModels;

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
            HasSchoolRecords = season.Meets.Any(m => m.Performances.Any(p => p.SchoolRecord)),
            SchoolRecords = season.Meets
                .SelectMany(m => m.Performances)
                .Where(p => p.SchoolRecord)
                .Select(p => p.Event.Name)
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