// MeetService.cs
using CloverleafTrack.DataAccess.Dtos;
using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using CloverleafTrack.Models.Enums;
using CloverleafTrack.Services.Interfaces;
using CloverleafTrack.ViewModels.Meets;
using Slugify;

namespace CloverleafTrack.Services;

public class MeetService(
    IMeetRepository meetRepository,
    IMeetPlacingRepository meetPlacingRepository) : IMeetService
{
    public async Task<MeetsIndexViewModel> GetMeetsIndexAsync()
    {
        var allMeets = await meetRepository.GetAllMeetsWithStatsAsync();

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
            foreach (var meet in meets)
            {
                var athleteCount = await meetRepository.GetUniqueAthleteCountForMeetAsync(meet.Id);
                var performanceCount = await meetRepository.GetPerformanceCountForMeetAsync(meet.Id);

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
                    SchoolRecordCount = meet.SchoolRecordCount
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

        // Group by gender
        var boysPerformances = performances.Where(p => p.EventGender == Gender.Male).ToList();
        var girlsPerformances = performances.Where(p => p.EventGender == Gender.Female).ToList();
        var mixedPerformances = performances.Where(p => p.EventGender == Gender.Mixed).ToList();

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
            Participants = participants,
            HasScoring = placings.Count > 0,

            // Stats
            TotalPerformances = performances.Count,
            TotalPRs = performances.Count(p => p.PersonalBest),
            TotalSchoolRecords = performances.Count(p => p.AllTimeRank == 1),
            UniqueAthletes = uniqueAthletes,

            BoysEvents = BuildOrderedEventGroups(boysPerformances, placingLookup),
            GirlsEvents = BuildOrderedEventGroups(girlsPerformances, placingLookup),
            MixedEvents = BuildOrderedEventGroups(mixedPerformances, placingLookup)
        };
    }

    private List<MeetEventGroupViewModel> BuildOrderedEventGroups(
        List<MeetPerformanceDto> performances,
        Dictionary<int, List<MeetPlacing>> placingLookup)
    {
        // Separate relays from non-relays based on event name
        var relays = performances.Where(p => IsRelay(p.EventName)).ToList();
        var nonRelays = performances.Where(p => !IsRelay(p.EventName)).ToList();

        // Group relays by their type
        var runningRelays = relays.Where(p => IsRunningRelay(p.EventType)).ToList();
        var jumpRelays = relays.Where(p => IsJumpRelay(p.EventType)).ToList();
        var throwRelays = relays.Where(p => IsThrowsRelay(p.EventType)).ToList();

        var eventGroups = new List<MeetEventGroupViewModel>();

        // 1. Sprints
        AddEventGroupsForCategory(eventGroups, nonRelays, EventCategory.Sprints, placingLookup);

        // 2. Distance
        AddEventGroupsForCategory(eventGroups, nonRelays, EventCategory.Distance, placingLookup);

        // 3. Hurdles
        AddEventGroupsForCategory(eventGroups, nonRelays, EventCategory.Hurdles, placingLookup);

        // 4. Running Relays
        AddEventGroupsFromList(eventGroups, runningRelays, placingLookup);

        // 5. Jumps
        AddEventGroupsForCategory(eventGroups, nonRelays, EventCategory.Jumps, placingLookup);

        // 6. Jump Relays
        AddEventGroupsFromList(eventGroups, jumpRelays, placingLookup);

        // 7. Throws
        AddEventGroupsForCategory(eventGroups, nonRelays, EventCategory.Throws, placingLookup);

        // 8. Throw Relays
        AddEventGroupsFromList(eventGroups, throwRelays, placingLookup);

        return eventGroups;
    }

    private void AddEventGroupsForCategory(
        List<MeetEventGroupViewModel> eventGroups,
        List<MeetPerformanceDto> performances,
        EventCategory category,
        Dictionary<int, List<MeetPlacing>> placingLookup)
    {
        var categoryPerformances = performances
            .Where(p => p.EventCategory == category)
            .GroupBy(p => new { p.EventId, p.EventName, p.EventCategory, p.EventSortOrder })
            .OrderBy(g => g.Key.EventSortOrder);

        foreach (var group in categoryPerformances)
        {
            eventGroups.Add(new MeetEventGroupViewModel
            {
                EventId = group.Key.EventId,
                EventName = group.Key.EventName,
                EventCategory = group.Key.EventCategory,
                Performances = group.Select(p => BuildPerformanceViewModel(p, placingLookup)).ToList()
            });
        }
    }

    private void AddEventGroupsFromList(
        List<MeetEventGroupViewModel> eventGroups,
        List<MeetPerformanceDto> performances,
        Dictionary<int, List<MeetPlacing>> placingLookup)
    {
        var grouped = performances
            .GroupBy(p => new { p.EventId, p.EventName, p.EventCategory, p.EventSortOrder })
            .OrderBy(g => g.Key.EventSortOrder);

        foreach (var group in grouped)
        {
            eventGroups.Add(new MeetEventGroupViewModel
            {
                EventId = group.Key.EventId,
                EventName = group.Key.EventName,
                EventCategory = group.Key.EventCategory,
                Performances = group.Select(p => BuildPerformanceViewModel(p, placingLookup)).ToList()
            });
        }
    }

    private static MeetPerformanceViewModel BuildPerformanceViewModel(
        MeetPerformanceDto p,
        Dictionary<int, List<MeetPlacing>> placingLookup)
    {
        var slugHelper = new SlugHelper();
        var isIndividual = p.AthleteId.HasValue;

        var vm = new MeetPerformanceViewModel
        {
            AthleteName = p.AthleteName,
            AthleteSlug = isIndividual ? slugHelper.GenerateSlug(p.AthleteName) : null,
            Performance = FormatPerformance(p.TimeSeconds, p.DistanceInches),
            IsPersonalBest = p.PersonalBest,
            IsSchoolRecord = p.AllTimeRank == 1,
            IsSeasonBest = p.SeasonBest,
            AllTimeRank = p.AllTimeRank
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

    private static bool IsRelay(string eventName)
    {
        return eventName.Contains("Relay", StringComparison.OrdinalIgnoreCase) ||
               eventName.Contains("4x", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsRunningRelay(EventType eventType) => eventType == EventType.RunningRelay;
    private static bool IsJumpRelay(EventType eventType) => eventType == EventType.JumpRelay;
    private static bool IsThrowsRelay(EventType eventType) => eventType == EventType.ThrowsRelay;

    private static string FormatPerformance(double? timeSeconds, double? distanceInches)
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
