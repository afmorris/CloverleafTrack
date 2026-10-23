using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Services.Interfaces;
using CloverleafTrack.ViewModels.Seasons;

namespace CloverleafTrack.Services;

public class SeasonWrapService(
    ISeasonRepository seasonRepository,
    ISeasonWrapRepository wrapRepository,
    IMeetService meetService,
    IScoringService scoringService) : ISeasonWrapService
{
    public async Task<SeasonWrapViewModel?> GetSeasonWrapAsync(string seasonName)
    {
        var season = await seasonRepository.GetByNameAsync(seasonName);
        if (season == null) return null;

        var marks = await wrapRepository.GetMarksAsync(season.Id);
        if (marks.Count == 0) return SeasonWrapCalculator.Build(season, marks, [], [], new Dictionary<int, ViewModels.Meets.MeetScoreViewModel>(), null);

        var priors = await wrapRepository.GetPriorBestsAsync(season.Id, season.StartDate);
        var teamResults = await wrapRepository.GetTeamResultsAsync(season.Id);
        var meetScores = await meetService.GetMeetScoresAsync();
        var scoring = season.ScoringEnabled ? await scoringService.GetSeasonScoringAsync(season.Id) : null;

        return SeasonWrapCalculator.Build(season, marks, priors, teamResults, meetScores, scoring);
    }
}
