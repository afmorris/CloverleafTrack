using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models.Enums;
using CloverleafTrack.Services.Interfaces;
using CloverleafTrack.ViewModels.Home;
using Slugify;

namespace CloverleafTrack.Services;

public class HomeService(
    IHomeRepository homeRepository,
    ISeasonRepository seasonRepository) : IHomeService
{
    private readonly SlugHelper _slugHelper = new();

    public async Task<HomePageViewModel> GetHomePageDataAsync()
    {
        // Get current season
        var allSeasons = await seasonRepository.GetAllAsync();
        var currentSeason = allSeasons.FirstOrDefault(s => s.IsCurrentSeason);
        
        if (currentSeason == null)
        {
            throw new InvalidOperationException("No current season found.");
        }

        var viewModel = new HomePageViewModel();

        // Get season stats
        var stats = await homeRepository.GetHomePageStatsAsync(currentSeason.Id);
        viewModel.TotalPRsThisSeason = stats.TotalPRsThisSeason;
        viewModel.SchoolRecordsBroken = stats.SchoolRecordsBroken;
        viewModel.ActiveAthletes = stats.ActiveAthletes;
        viewModel.MeetsCompleted = stats.MeetsCompleted;
        viewModel.TotalMeetsThisSeason = stats.TotalMeetsThisSeason;

        // Get "On This Day" (only if there's a performance)
        var today = DateTime.Today;
        var onThisDay = await homeRepository.GetPerformanceOnThisDayAsync(today.Month, today.Day);
        if (onThisDay != null)
        {
            viewModel.OnThisDay = new OnThisDayViewModel
            {
                EventName = onThisDay.EventName,
                Performance = FormatPerformance(onThisDay.TimeSeconds, onThisDay.DistanceInches),
                AthleteFirstName = onThisDay.AthleteFirstName,
                AthleteLastName = onThisDay.AthleteLastName,
                AthleteSlug = _slugHelper.GenerateSlug($"{onThisDay.AthleteFirstName}-{onThisDay.AthleteLastName}"),
                MeetName = onThisDay.MeetName,
                Date = onThisDay.Date,
                IsSchoolRecord = onThisDay.IsSchoolRecord,
                AllTimeRank = onThisDay.AllTimeRank
            };
        }

        // Get recent top performance
        var topPerf = await homeRepository.GetRecentTopPerformanceAsync(currentSeason.Id);
        if (topPerf != null)
        {
            viewModel.TopPerformance = new RecentHighlightViewModel
            {
                Performance = FormatPerformance(topPerf.TimeSeconds, topPerf.DistanceInches),
                EventName = topPerf.EventName,
                AthleteFirstName = topPerf.AthleteFirstName,
                AthleteLastName = topPerf.AthleteLastName,
                AthleteSlug = _slugHelper.GenerateSlug($"{topPerf.AthleteFirstName}-{topPerf.AthleteLastName}"),
                MeetName = topPerf.MeetName,
                Date = topPerf.Date,
                IsPersonalBest = topPerf.IsPersonalBest,
                IsSchoolRecord = topPerf.IsSchoolRecord
            };
        }

        // Get biggest improvement
        var improvement = await homeRepository.GetBiggestImprovementThisSeasonAsync(currentSeason.Id);
        if (improvement != null)
        {
            var currentYear = await GetCurrentSeasonYearAsync();
            
            viewModel.BiggestImprovement = new ImprovementViewModel
            {
                EventName = improvement.EventName,
                AthleteFirstName = improvement.AthleteFirstName,
                AthleteLastName = improvement.AthleteLastName,
                AthleteSlug = _slugHelper.GenerateSlug($"{improvement.AthleteFirstName}-{improvement.AthleteLastName}"),
                ImprovementDisplay = FormatImprovement(improvement.ImprovementAmount),
                PreviousPerformance = FormatPerformance(improvement.PreviousTimeSeconds, improvement.PreviousDistanceInches),
                CurrentPerformance = FormatPerformance(improvement.CurrentTimeSeconds, improvement.CurrentDistanceInches)
            };
        }

        // Get breakout athlete
        var breakout = await homeRepository.GetBreakoutAthleteAsync(currentSeason.Id);
        if (breakout != null)
        {
            var currentYear = await GetCurrentSeasonYearAsync();
            
            viewModel.BreakoutAthlete = new BreakoutAthleteViewModel
            {
                FirstName = breakout.FirstName,
                LastName = breakout.LastName,
                Slug = _slugHelper.GenerateSlug($"{breakout.FirstName}-{breakout.LastName}"),
                PRCount = breakout.PRCount,
                Class = GraduationYearToClass(breakout.GraduationYear, currentYear)
            };
        }

        // Get season leaders for boys
        var boysLeaders = await homeRepository.GetSeasonLeadersAsync(Gender.Male, currentSeason.Id);
        viewModel.BoysLeaders = boysLeaders.Select(l => new SeasonLeaderViewModel
        {
            EventName = l.EventName,
            Performance = FormatPerformance(l.TimeSeconds, l.DistanceInches),
            AthleteFirstName = l.AthleteFirstName,
            AthleteLastName = l.AthleteLastName,
            AthleteSlug = _slugHelper.GenerateSlug($"{l.AthleteFirstName}-{l.AthleteLastName}"),
            AllTimeRank = l.AllTimeRank
        }).ToList();

        // Get season leaders for girls
        var girlsLeaders = await homeRepository.GetSeasonLeadersAsync(Gender.Female, currentSeason.Id);
        viewModel.GirlsLeaders = girlsLeaders.Select(l => new SeasonLeaderViewModel
        {
            EventName = l.EventName,
            Performance = FormatPerformance(l.TimeSeconds, l.DistanceInches),
            AthleteFirstName = l.AthleteFirstName,
            AthleteLastName = l.AthleteLastName,
            AthleteSlug = _slugHelper.GenerateSlug($"{l.AthleteFirstName}-{l.AthleteLastName}"),
            AllTimeRank = l.AllTimeRank
        }).ToList();

        // Get upcoming meets
        var upcomingMeets = await homeRepository.GetUpcomingMeetsAsync();
        viewModel.UpcomingMeets = upcomingMeets.Select(m => new UpcomingMeetViewModel
        {
            Id = m.Id,
            Name = m.Name,
            Date = m.Date,
            Environment = m.Environment,
            Location = m.Location,
            Slug = _slugHelper.GenerateSlug(m.Name)
        }).ToList();

        return viewModel;
    }

    private async Task<int> GetCurrentSeasonYearAsync()
    {
        var allSeasons = await seasonRepository.GetAllAsync();
        var currentSeason = allSeasons.FirstOrDefault(s => s.IsCurrentSeason);
        return currentSeason?.EndDate.Year ?? DateTime.Now.Year;
    }

    private static string FormatPerformance(double? timeSeconds, double? distanceInches)
    {
        if (timeSeconds.HasValue)
        {
            return FormatTime(timeSeconds.Value);
        }
        else if (distanceInches.HasValue)
        {
            return FormatDistance(distanceInches.Value);
        }
        else
        {
            return "N/A";
        }
    }

    private static string FormatDistance(double inches)
    {
        var feet = Math.Floor(inches / 12);
        var remaining = inches % 12;
        return $"{feet:0}' {remaining:0.##}\"";
    }

    private static string FormatTime(double seconds)
    {
        if (seconds >= 60)
        {
            var minutes = (int)(seconds / 60);
            var remainder = seconds % 60;
            return $"{minutes}:{remainder:00.00}";
        }

        return $"{seconds:0.00}";
    }

    private static string FormatImprovement(double improvementAmount)
    {
        // For time events, improvement is negative (faster)
        // For field events, improvement is positive (farther)
        if (improvementAmount < 0)
        {
            return $"{Math.Abs(improvementAmount):0.00}s";
        }
        else
        {
            var feet = Math.Floor(improvementAmount / 12);
            var remaining = improvementAmount % 12;
            return $"{feet:0}' {remaining:0.##}\"";
        }
    }

    private static string GraduationYearToClass(int gradYear, int currentSeason)
    {
        var diff = gradYear - currentSeason;

        return diff switch
        {
            >= 3 => "Freshman",
            2 => "Sophomore",
            1 => "Junior",
            0 => "Senior",
            _ => $"{gradYear} Graduate"
        };
    }
}
