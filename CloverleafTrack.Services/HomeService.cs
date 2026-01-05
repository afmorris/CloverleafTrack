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

        // Get recent top performance - Outdoor
        var outdoorTopPerf = await homeRepository.GetRecentTopPerformanceAsync(currentSeason.Id, CloverleafTrack.Models.Enums.Environment.Outdoor);
        if (outdoorTopPerf != null)
        {
            viewModel.OutdoorTopPerformance = new RecentHighlightViewModel
            {
                Performance = FormatPerformance(outdoorTopPerf.TimeSeconds, outdoorTopPerf.DistanceInches),
                EventName = outdoorTopPerf.EventName,
                AthleteFirstName = outdoorTopPerf.AthleteFirstName,
                AthleteLastName = outdoorTopPerf.AthleteLastName,
                AthleteSlug = _slugHelper.GenerateSlug($"{outdoorTopPerf.AthleteFirstName}-{outdoorTopPerf.AthleteLastName}"),
                MeetName = outdoorTopPerf.MeetName,
                Date = outdoorTopPerf.Date,
                IsPersonalBest = outdoorTopPerf.IsPersonalBest,
                IsSchoolRecord = outdoorTopPerf.IsSchoolRecord,
                Environment = outdoorTopPerf.Environment
            };
        }

        // Get recent top performance - Indoor
        var indoorTopPerf = await homeRepository.GetRecentTopPerformanceAsync(currentSeason.Id, CloverleafTrack.Models.Enums.Environment.Indoor);
        if (indoorTopPerf != null)
        {
            viewModel.IndoorTopPerformance = new RecentHighlightViewModel
            {
                Performance = FormatPerformance(indoorTopPerf.TimeSeconds, indoorTopPerf.DistanceInches),
                EventName = indoorTopPerf.EventName,
                AthleteFirstName = indoorTopPerf.AthleteFirstName,
                AthleteLastName = indoorTopPerf.AthleteLastName,
                AthleteSlug = _slugHelper.GenerateSlug($"{indoorTopPerf.AthleteFirstName}-{indoorTopPerf.AthleteLastName}"),
                MeetName = indoorTopPerf.MeetName,
                Date = indoorTopPerf.Date,
                IsPersonalBest = indoorTopPerf.IsPersonalBest,
                IsSchoolRecord = indoorTopPerf.IsSchoolRecord,
                Environment = indoorTopPerf.Environment
            };
        }

        // Get biggest improvement - Outdoor
        var outdoorImprovement = await homeRepository.GetBiggestImprovementThisSeasonAsync(currentSeason.Id, CloverleafTrack.Models.Enums.Environment.Outdoor);
        if (outdoorImprovement != null)
        {
            var currentYear = await GetCurrentSeasonYearAsync();

            viewModel.OutdoorBiggestImprovement = new ImprovementViewModel
            {
                EventName = outdoorImprovement.EventName,
                AthleteFirstName = outdoorImprovement.AthleteFirstName,
                AthleteLastName = outdoorImprovement.AthleteLastName,
                AthleteSlug = _slugHelper.GenerateSlug($"{outdoorImprovement.AthleteFirstName}-{outdoorImprovement.AthleteLastName}"),
                ImprovementDisplay = FormatImprovement(outdoorImprovement.ImprovementAmount),
                PreviousPerformance = FormatPerformance(outdoorImprovement.PreviousTimeSeconds, outdoorImprovement.PreviousDistanceInches),
                CurrentPerformance = FormatPerformance(outdoorImprovement.CurrentTimeSeconds, outdoorImprovement.CurrentDistanceInches),
                Environment = outdoorImprovement.Environment
            };
        }

        // Get biggest improvement - Indoor
        var indoorImprovement = await homeRepository.GetBiggestImprovementThisSeasonAsync(currentSeason.Id, CloverleafTrack.Models.Enums.Environment.Indoor);
        if (indoorImprovement != null)
        {
            var currentYear = await GetCurrentSeasonYearAsync();

            viewModel.IndoorBiggestImprovement = new ImprovementViewModel
            {
                EventName = indoorImprovement.EventName,
                AthleteFirstName = indoorImprovement.AthleteFirstName,
                AthleteLastName = indoorImprovement.AthleteLastName,
                AthleteSlug = _slugHelper.GenerateSlug($"{indoorImprovement.AthleteFirstName}-{indoorImprovement.AthleteLastName}"),
                ImprovementDisplay = FormatImprovement(indoorImprovement.ImprovementAmount),
                PreviousPerformance = FormatPerformance(indoorImprovement.PreviousTimeSeconds, indoorImprovement.PreviousDistanceInches),
                CurrentPerformance = FormatPerformance(indoorImprovement.CurrentTimeSeconds, indoorImprovement.CurrentDistanceInches),
                Environment = indoorImprovement.Environment
            };
        }

        // Get breakout athlete - Outdoor
        var outdoorBreakout = await homeRepository.GetBreakoutAthleteAsync(currentSeason.Id, CloverleafTrack.Models.Enums.Environment.Outdoor);
        if (outdoorBreakout != null)
        {
            var currentYear = await GetCurrentSeasonYearAsync();

            viewModel.OutdoorBreakoutAthlete = new BreakoutAthleteViewModel
            {
                FirstName = outdoorBreakout.FirstName,
                LastName = outdoorBreakout.LastName,
                Slug = _slugHelper.GenerateSlug($"{outdoorBreakout.FirstName}-{outdoorBreakout.LastName}"),
                PRCount = outdoorBreakout.PRCount,
                Class = GraduationYearToClass(outdoorBreakout.GraduationYear, currentYear),
                Environment = outdoorBreakout.Environment
            };
        }

        // Get breakout athlete - Indoor
        var indoorBreakout = await homeRepository.GetBreakoutAthleteAsync(currentSeason.Id, CloverleafTrack.Models.Enums.Environment.Indoor);
        if (indoorBreakout != null)
        {
            var currentYear = await GetCurrentSeasonYearAsync();

            viewModel.IndoorBreakoutAthlete = new BreakoutAthleteViewModel
            {
                FirstName = indoorBreakout.FirstName,
                LastName = indoorBreakout.LastName,
                Slug = _slugHelper.GenerateSlug($"{indoorBreakout.FirstName}-{indoorBreakout.LastName}"),
                PRCount = indoorBreakout.PRCount,
                Class = GraduationYearToClass(indoorBreakout.GraduationYear, currentYear),
                Environment = indoorBreakout.Environment
            };
        }

        // Get season leaders - Boys Outdoor
        var boysOutdoorLeaders = await homeRepository.GetSeasonLeadersAsync(Gender.Male, currentSeason.Id, CloverleafTrack.Models.Enums.Environment.Outdoor);
        viewModel.BoysOutdoorLeaders = boysOutdoorLeaders.Select(l => new SeasonLeaderViewModel
        {
            EventName = l.EventName,
            Performance = FormatPerformance(l.TimeSeconds, l.DistanceInches),
            AthleteFirstName = l.AthleteFirstName,
            AthleteLastName = l.AthleteLastName,
            AthleteSlug = _slugHelper.GenerateSlug($"{l.AthleteFirstName}-{l.AthleteLastName}"),
            AllTimeRank = l.AllTimeRank
        }).ToList();

        // Get season leaders - Boys Indoor
        var boysIndoorLeaders = await homeRepository.GetSeasonLeadersAsync(Gender.Male, currentSeason.Id, CloverleafTrack.Models.Enums.Environment.Indoor);
        viewModel.BoysIndoorLeaders = boysIndoorLeaders.Select(l => new SeasonLeaderViewModel
        {
            EventName = l.EventName,
            Performance = FormatPerformance(l.TimeSeconds, l.DistanceInches),
            AthleteFirstName = l.AthleteFirstName,
            AthleteLastName = l.AthleteLastName,
            AthleteSlug = _slugHelper.GenerateSlug($"{l.AthleteFirstName}-{l.AthleteLastName}"),
            AllTimeRank = l.AllTimeRank
        }).ToList();

        // Get season leaders - Girls Outdoor
        var girlsOutdoorLeaders = await homeRepository.GetSeasonLeadersAsync(Gender.Female, currentSeason.Id, CloverleafTrack.Models.Enums.Environment.Outdoor);
        viewModel.GirlsOutdoorLeaders = girlsOutdoorLeaders.Select(l => new SeasonLeaderViewModel
        {
            EventName = l.EventName,
            Performance = FormatPerformance(l.TimeSeconds, l.DistanceInches),
            AthleteFirstName = l.AthleteFirstName,
            AthleteLastName = l.AthleteLastName,
            AthleteSlug = _slugHelper.GenerateSlug($"{l.AthleteFirstName}-{l.AthleteLastName}"),
            AllTimeRank = l.AllTimeRank
        }).ToList();

        // Get season leaders - Girls Indoor
        var girlsIndoorLeaders = await homeRepository.GetSeasonLeadersAsync(Gender.Female, currentSeason.Id, CloverleafTrack.Models.Enums.Environment.Indoor);
        viewModel.GirlsIndoorLeaders = girlsIndoorLeaders.Select(l => new SeasonLeaderViewModel
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