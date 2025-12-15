// MeetService.cs
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
            UniqueAthletes = performances.Where(p => p.AthleteId.HasValue).Select(p => p.AthleteId).Distinct().Count(),
            
            // Boys events
            BoysEvents = boysPerformances
                .GroupBy(p => new { p.EventId, p.EventName, p.EventCategory, p.EventSortOrder })
                .OrderBy(g => g.Key.EventSortOrder)
                .Select(g => new MeetEventGroupViewModel
                {
                    EventId = g.Key.EventId,
                    EventName = g.Key.EventName,
                    EventCategory = g.Key.EventCategory,
                    Performances = g.Select(p => new MeetPerformanceViewModel
                    {
                        AthleteName = p.AthleteName,
                        Performance = FormatPerformance(p.TimeSeconds, p.DistanceInches),
                        IsPersonalBest = p.PersonalBest,
                        IsSchoolRecord = p.SchoolRecord,
                        IsSeasonBest = p.SeasonBest,
                        AllTimeRank = p.AllTimeRank
                    }).ToList()
                }).ToList(),
            
            // Girls events
            GirlsEvents = girlsPerformances
                .GroupBy(p => new { p.EventId, p.EventName, p.EventCategory, p.EventSortOrder })
                .OrderBy(g => g.Key.EventSortOrder)
                .Select(g => new MeetEventGroupViewModel
                {
                    EventId = g.Key.EventId,
                    EventName = g.Key.EventName,
                    EventCategory = g.Key.EventCategory,
                    Performances = g.Select(p => new MeetPerformanceViewModel
                    {
                        AthleteName = p.AthleteName,
                        Performance = FormatPerformance(p.TimeSeconds, p.DistanceInches),
                        IsPersonalBest = p.PersonalBest,
                        IsSchoolRecord = p.SchoolRecord,
                        IsSeasonBest = p.SeasonBest,
                        AllTimeRank = p.AllTimeRank
                    }).ToList()
                }).ToList()
        };
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