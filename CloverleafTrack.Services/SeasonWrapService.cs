using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Services.Interfaces;
using CloverleafTrack.ViewModels.Seasons;

namespace CloverleafTrack.Services;

public class SeasonWrapService(
    ISeasonRepository seasonRepository,
    ISeasonWrapRepository wrapRepository,
    IMeetService meetService,
    IScoringService scoringService,
    ISeasonSummaryRepository summaryRepository) : ISeasonWrapService
{
    public async Task<SeasonWrapViewModel?> GetSeasonWrapAsync(string seasonName)
    {
        var season = await seasonRepository.GetByNameAsync(seasonName);
        if (season == null) return null;

        var marks = await wrapRepository.GetMarksAsync(season.Id);
        var (previous, next) = await NeighboursAsync(season);
        if (marks.Count == 0)
        {
            var empty = SeasonWrapCalculator.Build(season, marks, [], [], new Dictionary<int, ViewModels.Meets.MeetScoreViewModel>(), null);
            empty.PreviousSeasonName = previous;
            empty.NextSeasonName = next;
            return empty;
        }

        var priors = await wrapRepository.GetPriorBestsAsync(season.Id, season.StartDate);
        var teamResults = await wrapRepository.GetTeamResultsAsync(season.Id);
        var meetScores = await meetService.GetMeetScoresAsync();
        var scoring = season.ScoringEnabled ? await scoringService.GetSeasonScoringAsync(season.Id) : null;

        var vm = SeasonWrapCalculator.Build(season, marks, priors, teamResults, meetScores, scoring);
        vm.PreviousSeasonName = previous;
        vm.NextSeasonName = next;
        vm.SummaryText = await summaryRepository.GetApprovedTextAsync(season.Id);
        return vm;
    }

    /// <summary>The closest older and newer seasons that have results (so the links never lead to an empty wrap).</summary>
    private async Task<(string? Previous, string? Next)> NeighboursAsync(Models.Season season)
    {
        var withData = (await wrapRepository.GetSeasonIdsWithMarksAsync()).ToHashSet();
        var seasons = (await seasonRepository.GetAllAsync()).Where(s => withData.Contains(s.Id)).OrderBy(s => s.StartDate).ToList();
        return (seasons.LastOrDefault(s => s.StartDate < season.StartDate)?.Name, seasons.FirstOrDefault(s => s.StartDate > season.StartDate)?.Name);
    }
}
