using CloverleafTrack.DataAccess.Dtos;
using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models.Enums;
using CloverleafTrack.Services.Interfaces;
using CloverleafTrack.ViewModels.Leaderboard;
using Slugify;
using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.Services;

public class LeaderboardService(ILeaderboardRepository leaderboardRepository) : ILeaderboardService
{
    private readonly SlugHelper _slugHelper = new();

    public async Task<LeaderboardViewModel> GetLeaderboardAsync()
    {
        var allPerformances = await leaderboardRepository.GetTopPerformancePerEventAsync();

        var viewModel = new LeaderboardViewModel
        {
            BoysOutdoorCategories = BuildCategoryViewModels(
                allPerformances.Where(p => p.Gender == Gender.Male && p.Environment == Environment.Outdoor).ToList()
            ),
            BoysIndoorCategories = BuildCategoryViewModels(
                allPerformances.Where(p => p.Gender == Gender.Male && p.Environment == Environment.Indoor).ToList()
            ),
            GirlsOutdoorCategories = BuildCategoryViewModels(
                allPerformances.Where(p => p.Gender == Gender.Female && p.Environment == Environment.Outdoor).ToList()
            ),
            GirlsIndoorCategories = BuildCategoryViewModels(
                allPerformances.Where(p => p.Gender == Gender.Female && p.Environment == Environment.Indoor).ToList()
            )
        };

        return viewModel;
    }

    private List<LeaderboardCategoryViewModel> BuildCategoryViewModels(List<LeaderboardDto> performances)
    {
        return performances
            .GroupBy(p => p.EventCategory)
            .OrderBy(g => g.First().EventCategorySortOrder)
            .Select(categoryGroup => new LeaderboardCategoryViewModel
            {
                Category = categoryGroup.Key,
                CategoryName = GetCategoryName(categoryGroup.Key),
                Events = categoryGroup
                    .OrderBy(p => p.EventSortOrder)
                    .Select(p => new LeaderboardEventViewModel
                    {
                        EventId = p.EventId,
                        EventName = p.EventName,
                        EventKey = p.EventKey,
                        Gender = p.Gender,
                        Environment = p.Environment,
                        AthleteFirstName = p.AthleteFirstName ?? "",
                        AthleteLastName = p.AthleteLastName ?? "",
                        RelayName = p.RelayName ?? "",
                        AthleteSlug = p.AthleteSlug ?? "",
                        Performance = FormatPerformance(p.TimeSeconds, p.DistanceInches),
                        MeetDate = p.PerformanceId > 0 ? p.MeetDate : null,
                        MeetName = p.MeetName ?? "",
                        MeetSlug = p.MeetSlug ?? ""
                    })
                    .ToList()
            })
            .ToList();
    }

    private static string GetCategoryName(EventCategory category)
    {
        return category switch
        {
            EventCategory.Sprints => "Sprints",
            EventCategory.Distance => "Distance",
            EventCategory.Hurdles => "Hurdles",
            EventCategory.Jumps => "Jumps",
            EventCategory.Throws => "Throws",
            _ => "Other"
        };
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
            return string.Empty;
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
}