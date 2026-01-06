// MeetService.cs
using CloverleafTrack.DataAccess.Dtos;
using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models.Enums;
using CloverleafTrack.Services.Interfaces;
using CloverleafTrack.ViewModels.Meets;

namespace CloverleafTrack.Services;

public class MeetService(IMeetRepository meetRepository) : IMeetService
{
    public async Task<MeetDetailsViewModel?> GetMeetDetailsAsync(string slug)
    {
        // Get basic meet info
        var meet = await meetRepository.GetMeetBasicInfoBySlugAsync(slug);
        if (meet == null)
        {
            return null;
        }
        
        // Get all performances
        var performances = await meetRepository.GetPerformancesForMeetAsync(meet.Id);
        
        // Get unique athlete count (includes relay athletes)
        var uniqueAthletes = await meetRepository.GetUniqueAthleteCountForMeetAsync(meet.Id);
        
        // Group by gender
        var boysPerformances = performances.Where(p => p.EventGender == Gender.Male).ToList();
        var girlsPerformances = performances.Where(p => p.EventGender == Gender.Female).ToList();
        
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
            
            // Stats
            TotalPerformances = performances.Count,
            TotalPRs = performances.Count(p => p.PersonalBest),
            TotalSchoolRecords = performances.Count(p => p.SchoolRecord),
            UniqueAthletes = uniqueAthletes,  // Now includes relay athletes!
            
            // Boys events - using relay-aware ordering
            BoysEvents = BuildOrderedEventGroups(boysPerformances),
            
            // Girls events - using relay-aware ordering
            GirlsEvents = BuildOrderedEventGroups(girlsPerformances)
        };
    }
    
    private List<MeetEventGroupViewModel> BuildOrderedEventGroups(List<MeetPerformanceDto> performances)
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
        AddEventGroupsForCategory(eventGroups, nonRelays, EventCategory.Sprints);
        
        // 2. Distance
        AddEventGroupsForCategory(eventGroups, nonRelays, EventCategory.Distance);
        
        // 3. Hurdles
        AddEventGroupsForCategory(eventGroups, nonRelays, EventCategory.Hurdles);
        
        // 4. Running Relays
        AddEventGroupsFromList(eventGroups, runningRelays);
        
        // 5. Jumps
        AddEventGroupsForCategory(eventGroups, nonRelays, EventCategory.Jumps);
        
        // 6. Jump Relays
        AddEventGroupsFromList(eventGroups, jumpRelays);
        
        // 7. Throws
        AddEventGroupsForCategory(eventGroups, nonRelays, EventCategory.Throws);
        
        // 8. Throw Relays
        AddEventGroupsFromList(eventGroups, throwRelays);
        
        return eventGroups;
    }
    
    private void AddEventGroupsForCategory(
        List<MeetEventGroupViewModel> eventGroups,
        List<MeetPerformanceDto> performances,
        EventCategory category)
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
                Performances = group.Select(p => new MeetPerformanceViewModel
                {
                    AthleteName = p.AthleteName,
                    Performance = FormatPerformance(p.TimeSeconds, p.DistanceInches),
                    IsPersonalBest = p.PersonalBest,
                    IsSchoolRecord = p.SchoolRecord,
                    IsSeasonBest = p.SeasonBest,
                    AllTimeRank = p.AllTimeRank
                }).ToList()
            });
        }
    }
    
    private void AddEventGroupsFromList(
        List<MeetEventGroupViewModel> eventGroups,
        List<MeetPerformanceDto> performances)
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
                Performances = group.Select(p => new MeetPerformanceViewModel
                {
                    AthleteName = p.AthleteName,
                    Performance = FormatPerformance(p.TimeSeconds, p.DistanceInches),
                    IsPersonalBest = p.PersonalBest,
                    IsSchoolRecord = p.SchoolRecord,
                    IsSeasonBest = p.SeasonBest,
                    AllTimeRank = p.AllTimeRank
                }).ToList()
            });
        }
    }
    
    private static bool IsRelay(string eventName)
    {
        return eventName.Contains("Relay", StringComparison.OrdinalIgnoreCase) || 
               eventName.Contains("4x", StringComparison.OrdinalIgnoreCase);
    }
    
    private static bool IsRunningRelay(EventType eventType)
    {
        // Running relays are: generic Relays category, or from Sprints, Distance, or Hurdles
        return eventType == EventType.RunningRelay;
    }

    private static bool IsJumpRelay(EventType eventType)
    {
        // Running relays are: generic Relays category, or from Sprints, Distance, or Hurdles
        return eventType == EventType.JumpRelay;
    }

    private static bool IsThrowsRelay(EventType eventType)
    {
        // Running relays are: generic Relays category, or from Sprints, Distance, or Hurdles
        return eventType == EventType.ThrowsRelay;
    }
    
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