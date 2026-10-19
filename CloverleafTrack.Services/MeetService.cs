// MeetService.cs
using CloverleafTrack.DataAccess.Dtos;
using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using CloverleafTrack.Models.Enums;
using CloverleafTrack.Services.Interfaces;
using CloverleafTrack.ViewModels.Meets;
using CloverleafTrack.ViewModels.Shared;
using Slugify;

namespace CloverleafTrack.Services;

public class MeetService(
    IMeetRepository meetRepository,
    IMeetPlacingRepository meetPlacingRepository,
    IPerformanceAttemptRepository? attemptRepository = null,
    IMeetSummaryRepository? summaryRepository = null,
    IPerformanceRoundRepository? roundRepository = null,
    IPerformanceBarRepository? barRepository = null) : IMeetService
{
    public async Task<MeetsIndexViewModel> GetMeetsIndexAsync()
    {
        var allMeets = await meetRepository.GetAllMeetsWithStatsAsync();
        var scores = await GetMeetScoresAsync();

        // Group by season
        var seasonGroups = allMeets
            .GroupBy(m => new { m.SeasonId, m.Season.Name, m.Season.StartDate })
            .OrderByDescending(g => g.Key.StartDate)
            .ToList();

        var seasons = new List<SeasonMeetsViewModel>();

        foreach (var seasonGroup in seasonGroups)
        {
            var meets = seasonGroup.OrderBy(m => m.Date).ToList();
            var completedMeets = meets.Where(m => m.Date <= DateTime.Now).ToList();

            var seasonMeets = new SeasonMeetsViewModel
            {
                SeasonName = seasonGroup.Key.Name,
                TotalMeets = meets.Count,
                CompletedMeets = completedMeets.Count,
                TotalPRs = meets.Sum(m => m.PRCount),
                TotalSchoolRecords = meets.Sum(m => m.SchoolRecordCount),
                IsCurrentSeason = DateTime.Now >= seasonGroup.First().Season.StartDate &&
                                  DateTime.Now <= seasonGroup.First().Season.EndDate,
                Meets = new List<MeetListItemViewModel>()
            };

            // Get athlete counts for each meet
            var allMeetIds = allMeets.Select(m => m.Id).ToList();
        var allTeamResults = await meetRepository.GetTeamResultsForMeetsAsync(allMeetIds);
        var teamResultsByMeet = allTeamResults.GroupBy(r => r.MeetId)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var meet in meets)
            {
                var athleteCount = await meetRepository.GetUniqueAthleteCountForMeetAsync(meet.Id);
                var performanceCount = await meetRepository.GetPerformanceCountForMeetAsync(meet.Id);

                teamResultsByMeet.TryGetValue(meet.Id, out var meetResults);
                var teamResult = BuildTeamResult(meet, meetResults ?? new());
                seasonMeets.Meets.Add(new MeetListItemViewModel
                {
                    Id = meet.Id,
                    Name = meet.Name,
                    Slug = meet.Slug,
                    Date = meet.Date,
                    Environment = meet.Environment,
                    LocationName = meet.Location.Name,
                    LocationCity = meet.Location.City ?? "",
                    LocationState = meet.Location.State ?? "",
                    AthleteCount = athleteCount,
                    PerformanceCount = performanceCount,
                    PRCount = meet.PRCount,
                    SchoolRecordCount = meet.SchoolRecordCount,
                    MeetType = meet.MeetType,
                    MeetScore = scores.GetValueOrDefault(meet.Id),
                    TeamResult = teamResult.HasResult ? teamResult : null
                });
            }

            seasons.Add(seasonMeets);
        }

        return new MeetsIndexViewModel
        {
            TotalMeets = allMeets.Count,
            TotalPRs = allMeets.Sum(m => m.PRCount),
            TotalSchoolRecords = allMeets.Sum(m => m.SchoolRecordCount),
            TotalSeasons = seasonGroups.Count,
            Seasons = seasons
        };
    }

    public async Task<MeetDetailsViewModel?> GetMeetDetailsAsync(string slug)
    {
        // Get basic meet info
        var meet = await meetRepository.GetMeetBasicInfoBySlugAsync(slug);
        if (meet == null)
        {
            return null;
        }

        // Parallel data loading
        var performancesTask = meetRepository.GetPerformancesForMeetAsync(meet.Id);
        var uniqueAthletesTask = meetRepository.GetUniqueAthleteCountForMeetAsync(meet.Id);
        var participantsTask = meetRepository.GetParticipantsForMeetAsync(meet.Id);
        var placingsTask = meetPlacingRepository.GetForMeetAsync(meet.Id);

        await Task.WhenAll(performancesTask, uniqueAthletesTask, participantsTask, placingsTask);

        var performances = performancesTask.Result;
        var uniqueAthletes = uniqueAthletesTask.Result;
        var participants = participantsTask.Result;
        var placings = placingsTask.Result;

        // Build a lookup: PerformanceId → placings
        var placingLookup = placings
            .GroupBy(p => p.PerformanceId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Build a lookup: PerformanceId → attempt series (empty/absent for performances with no recorded series)
        var attempts = attemptRepository != null
            ? await attemptRepository.GetAttemptsForPerformancesAsync(performances.Select(p => p.PerformanceId))
            : new List<PerformanceAttempt>();
        var attemptLookup = PerformanceAttemptSeriesBuilder.BuildLookup(attempts);

        // Group by gender
        var boysPerformances = performances.Where(p => p.EventGender == Gender.Male).ToList();
        var girlsPerformances = performances.Where(p => p.EventGender == Gender.Female).ToList();
        var mixedPerformances = performances.Where(p => p.EventGender == Gender.Mixed).ToList();

        var boysEvents = BuildOrderedEventGroups(boysPerformances, placingLookup, attemptLookup);
        var girlsEvents = BuildOrderedEventGroups(girlsPerformances, placingLookup, attemptLookup);
        var mixedEvents = BuildOrderedEventGroups(mixedPerformances, placingLookup, attemptLookup);

        // Attach prelim → final chains (absent for performances with a single round)
        var rounds = roundRepository != null
            ? await roundRepository.GetRoundsForPerformancesAsync(performances.Select(p => p.PerformanceId))
            : new List<PerformanceRound>();
        var roundLookup = PerformanceRoundSeriesBuilder.BuildLookup(rounds);
        var bars = barRepository != null
            ? await barRepository.GetBarsForPerformancesAsync(performances.Select(p => p.PerformanceId))
            : new List<PerformanceBar>();
        var barLookup = PerformanceBarSeriesBuilder.BuildLookup(bars);
        foreach (var perf in boysEvents.Concat(girlsEvents).Concat(mixedEvents).SelectMany(g => g.Performances))
        {
            perf.BarSeries = barLookup.GetValueOrDefault(perf.PerformanceId) ?? new PerformanceBarSeriesViewModel();
            perf.RoundSeries = roundLookup.GetValueOrDefault(perf.PerformanceId) ?? new PerformanceRoundSeriesViewModel();
        }
        var mappedPerformances = boysEvents
            .Concat(girlsEvents)
            .Concat(mixedEvents)
            .SelectMany(g => g.Performances)
            .ToList();

        var teamResultsTask = meetRepository.GetTeamResultsForMeetsAsync(new List<int> { meet.Id });
        await teamResultsTask;
        var teamResult = BuildTeamResult(meet, teamResultsTask.Result);
        var scores = await GetMeetScoresAsync();

        // Only reviewed text is ever shown publicly: a Draft summary must not reach the page.
        var summary = summaryRepository != null ? await summaryRepository.GetForMeetAsync(meet.Id) : null;
        var approvedSummary = summary is { Status: SummaryStatus.Approved } ? summary : null;

        return new MeetDetailsViewModel
        {
            Name = meet.Name,
            Date = meet.Date,
            LocationName = meet.Location.Name,
            LocationCity = meet.Location.City ?? "",
            LocationState = meet.Location.State ?? "",
            Environment = meet.Environment,
            HandTimed = meet.HandTimed,
            SeasonName = meet.Season.Name,
            MeetType = meet.MeetType,
            OfficialResultsUrl = IsHttpUrl(meet.OfficialResultsUrl) ? meet.OfficialResultsUrl : null,
            MeetScore = scores.GetValueOrDefault(meet.Id),
            SummaryText = approvedSummary?.SummaryText,
            SummaryHtml = approvedSummary == null ? null
                : MeetSummaryLinkService.Linkify(approvedSummary.SummaryText, performances, "text-blue-600 dark:text-blue-400 hover:underline"),
            Participants = participants,
            HasScoring = placings.Count > 0,
            TeamResult = teamResult.HasResult ? teamResult : null,

            // Stats
            TotalPerformances = performances.Count(p => p.ResultStatus == ResultStatus.Mark),
            TotalPRs = performances.Count(p => p.PersonalBest),
            TotalSchoolRecords = performances.Count(p => p.AllTimeRank == 1),
            UniqueAthletes = uniqueAthletes,
            TopTenAllTimeCount = mappedPerformances.Count(p => p.AllTimeRank is <= 10),
            SeasonBestCount = mappedPerformances.Count(p => p.IsSeasonBest),
            PlacingCount = mappedPerformances.Count(p => p.HasPlacing),
            HasMixedEvents = mixedEvents.Any(),

            BoysEvents = boysEvents,
            GirlsEvents = girlsEvents,
            MixedEvents = mixedEvents
        };
    }

    /// <summary>
    /// MeetScore needs every mark in the database (PR detection compares against all earlier meets), so scores are
    /// always computed for all meets together and looked up by meet id. See MeetScoreCalculator / BRAIN.md [C45].
    /// </summary>
    private async Task<Dictionary<int, MeetScoreViewModel>> GetMeetScoresAsync()
    {
        var marks = await meetRepository.GetMeetScoreMarksAsync() ?? new List<MeetScoreMark>();
        return MeetScoreCalculator.Calculate(marks).ToDictionary(
            kv => kv.Key,
            kv => new MeetScoreViewModel
            {
                Score = Math.Round(kv.Value.Score, 1),
                Basis = kv.Value.Basis,
                Peak = kv.Value.Peak.HasValue ? Math.Round(kv.Value.Peak.Value, 1) : null,
                Depth = kv.Value.Depth.HasValue ? Math.Round(kv.Value.Depth.Value, 1) : null,
                Improvement = kv.Value.Improvement.HasValue ? Math.Round(kv.Value.Improvement.Value, 1) : null,
                PRs = kv.Value.PRs,
                ExpectedPRs = Math.Round(kv.Value.ExpectedPRs, 2),
                EligibleAthleteEvents = kv.Value.EligibleAthleteEvents
            });
    }

    private static TeamResultViewModel BuildTeamResult(Meet meet, List<MeetTeamResult> teamResults)
    {
        var isDual = meet.MeetType == MeetType.Dual || meet.MeetType == MeetType.DoubleDual;
        var entries = teamResults
            .OrderBy(r => r.Gender)
            .ThenBy(r => r.OpponentName)
            .ThenBy(r => r.Id)
            .Select(r => new TeamResultEntryViewModel
            {
                Gender = r.Gender,
                OpponentName = r.OpponentName,
                OurScore = r.OurScore,
                OpponentScore = r.OpponentScore,
                Place = r.Place,
                FieldSize = r.FieldSize,
                IsDualStyle = isDual
            })
            .ToList();

        return new TeamResultViewModel { MeetType = meet.MeetType, Entries = entries };
    }

    private List<MeetEventGroupViewModel> BuildOrderedEventGroups(
        List<MeetPerformanceDto> performances,
        Dictionary<int, List<MeetPlacing>> placingLookup,
        Dictionary<int, PerformanceAttemptSeriesViewModel> attemptLookup)
    {
        // Separate relays from non-relays. EventType is authoritative: medley relays ("800M Sprint Medley") have no
        // "Relay"/"4x" in their names and were silently dropped when classification was name-only (see BRAIN.md [C50]).
        var relays = performances.Where(IsRelay).ToList();
        var nonRelays = performances.Where(p => !IsRelay(p)).ToList();

        // Group relays by their type
        var runningRelays = relays.Where(p => IsRunningRelay(p.EventType)).ToList();
        var jumpRelays = relays.Where(p => IsJumpRelay(p.EventType)).ToList();
        var throwRelays = relays.Where(p => IsThrowsRelay(p.EventType)).ToList();

        var eventGroups = new List<MeetEventGroupViewModel>();

        // 1. Sprints
        AddEventGroupsForCategory(eventGroups, nonRelays, EventCategory.Sprints, placingLookup, attemptLookup);

        // 2. Distance
        AddEventGroupsForCategory(eventGroups, nonRelays, EventCategory.Distance, placingLookup, attemptLookup);

        // 3. Hurdles
        AddEventGroupsForCategory(eventGroups, nonRelays, EventCategory.Hurdles, placingLookup, attemptLookup);

        // 4. Running Relays
        AddEventGroupsFromList(eventGroups, runningRelays, placingLookup, attemptLookup);

        // 5. Jumps
        AddEventGroupsForCategory(eventGroups, nonRelays, EventCategory.Jumps, placingLookup, attemptLookup);

        // 6. Jump Relays
        AddEventGroupsFromList(eventGroups, jumpRelays, placingLookup, attemptLookup);

        // 7. Throws
        AddEventGroupsForCategory(eventGroups, nonRelays, EventCategory.Throws, placingLookup, attemptLookup);

        // 8. Throw Relays
        AddEventGroupsFromList(eventGroups, throwRelays, placingLookup, attemptLookup);

        return eventGroups;
    }

    private void AddEventGroupsForCategory(
        List<MeetEventGroupViewModel> eventGroups,
        List<MeetPerformanceDto> performances,
        EventCategory category,
        Dictionary<int, List<MeetPlacing>> placingLookup,
        Dictionary<int, PerformanceAttemptSeriesViewModel> attemptLookup)
    {
        var categoryPerformances = performances
            .Where(p => p.EventCategory == category)
            .GroupBy(p => new { p.EventId, p.EventName, p.EventCategory, p.EventSortOrder, p.EventType })
            .OrderBy(g => g.Key.EventSortOrder);

        foreach (var group in categoryPerformances)
        {
            eventGroups.Add(new MeetEventGroupViewModel
            {
                EventId = group.Key.EventId,
                EventName = group.Key.EventName,
                EventCategory = group.Key.EventCategory,
                IsFieldEvent = IsFieldEventType(group.Key.EventType),
                Performances = group.Select(p => BuildPerformanceViewModel(p, placingLookup, attemptLookup)).ToList()
            });
        }
    }

    private void AddEventGroupsFromList(
        List<MeetEventGroupViewModel> eventGroups,
        List<MeetPerformanceDto> performances,
        Dictionary<int, List<MeetPlacing>> placingLookup,
        Dictionary<int, PerformanceAttemptSeriesViewModel> attemptLookup)
    {
        var grouped = performances
            .GroupBy(p => new { p.EventId, p.EventName, p.EventCategory, p.EventSortOrder, p.EventType })
            .OrderBy(g => g.Key.EventSortOrder);

        foreach (var group in grouped)
        {
            eventGroups.Add(new MeetEventGroupViewModel
            {
                EventId = group.Key.EventId,
                EventName = group.Key.EventName,
                EventCategory = group.Key.EventCategory,
                IsFieldEvent = IsFieldEventType(group.Key.EventType),
                Performances = group.Select(p => BuildPerformanceViewModel(p, placingLookup, attemptLookup)).ToList()
            });
        }
    }

    /// <summary>Field events (including field-relay variants) sort higher-is-better; running events sort lower-is-better.</summary>
    private static bool IsFieldEventType(EventType eventType) => eventType switch
    {
        EventType.Field or EventType.FieldRelay or EventType.JumpRelay or EventType.ThrowsRelay => true,
        _ => false
    };

    private static MeetPerformanceViewModel BuildPerformanceViewModel(
        MeetPerformanceDto p,
        Dictionary<int, List<MeetPlacing>> placingLookup,
        Dictionary<int, PerformanceAttemptSeriesViewModel> attemptLookup)
    {
        var slugHelper = new SlugHelper();
        var isIndividual = p.AthleteId.HasValue;

        var vm = new MeetPerformanceViewModel
        {
            PerformanceId = p.PerformanceId,
            AthleteName = p.AthleteName,
            AthleteSlug = isIndividual ? slugHelper.GenerateSlug(p.AthleteName) : null,
            Performance = p.ResultStatus == ResultStatus.Mark
                ? FormatPerformance(p.TimeSeconds, p.DistanceInches)
                : p.ResultStatus.Abbreviation(),
            ResultStatus = p.ResultStatus,
            IsPersonalBest = p.PersonalBest,
            IsSchoolRecord = p.AllTimeRank == 1,
            IsSeasonBest = p.SeasonBest,
            AllTimeRank = p.AllTimeRank,
            AttemptSeries = attemptLookup.GetValueOrDefault(p.PerformanceId) ?? new PerformanceAttemptSeriesViewModel(),
            RawValue = p.DistanceInches ?? p.TimeSeconds
        };

        if (placingLookup.TryGetValue(p.PerformanceId, out var perfPlacings))
        {
            vm.Placings = perfPlacings
                .OrderBy(pl => pl.MeetParticipantId ?? 0)
                .Select(pl => new PerformancePlacingViewModel
                {
                    Place = pl.Place,
                    FullPoints = pl.FullPoints,
                    SplitPoints = pl.SplitPoints,
                    OpponentSchoolName = pl.MeetParticipant?.SchoolName
                })
                .ToList();
        }

        return vm;
    }

    private static bool IsRelay(MeetPerformanceDto p) =>
        p.EventType is EventType.RunningRelay or EventType.FieldRelay or EventType.JumpRelay or EventType.ThrowsRelay
        || IsRelay(p.EventName);

    private static bool IsRelay(string eventName)
    {
        return eventName.Contains("Relay", StringComparison.OrdinalIgnoreCase) ||
               eventName.Contains("4x", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsRunningRelay(EventType eventType) => eventType == EventType.RunningRelay;
    private static bool IsJumpRelay(EventType eventType) => eventType == EventType.JumpRelay;
    private static bool IsThrowsRelay(EventType eventType) => eventType == EventType.ThrowsRelay;

    private static bool IsHttpUrl(string? url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

    internal static string FormatPerformance(double? timeSeconds, double? distanceInches)
    {
        if (distanceInches.HasValue)
        {
            var feet = Math.Floor(distanceInches.Value / 12);
            var remaining = distanceInches.Value % 12;
            return $"{feet:0}' {remaining:0.##}\"";
        }

        if (timeSeconds.HasValue)
        {
            var timeSpan = TimeSpan.FromSeconds(timeSeconds.Value);
            return timeSpan.TotalMinutes >= 1
                ? timeSpan.ToString(@"m\:ss\.ff")
                : timeSpan.ToString(@"s\.ff");
        }

        return "N/A";
    }
}
