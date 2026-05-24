using CloverleafTrack.DataAccess.Dtos;
using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using CloverleafTrack.Models.Enums;
using CloverleafTrack.Services;
using FluentAssertions;
using Moq;
using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.Tests.Unit.Services;

public class HomeServiceTests
{
    private readonly Mock<IHomeRepository> _homeRepository;
    private readonly Mock<ISeasonRepository> _seasonRepository;
    private readonly HomeService _service;

    public HomeServiceTests()
    {
        _homeRepository = new Mock<IHomeRepository>();
        _seasonRepository = new Mock<ISeasonRepository>();
        _service = new HomeService(_homeRepository.Object, _seasonRepository.Object);
    }

    [Fact]
    public async Task GetHomePageDataAsync_MapsLatestMeetImpact_WhenRepositoryReturnsMeet()
    {
        SetupDefaults();
        _homeRepository.Setup(r => r.GetLatestCompletedMeetImpactAsync(1))
            .ReturnsAsync(new LatestMeetImpactDto
            {
                MeetName = "County Meet",
                Date = new DateTime(2026, 4, 18),
                Environment = Environment.Outdoor,
                LocationName = "Cloverleaf Stadium",
                LocationCity = "Lodi",
                LocationState = "OH",
                TotalPerformances = 72,
                TotalPRs = 18,
                TotalSchoolRecords = 2,
                TopTenAllTimeMarks = 9,
                UniqueAthletes = 31
            });

        var result = await _service.GetHomePageDataAsync();

        result.LatestMeetImpact.Should().NotBeNull();
        result.LatestMeetImpact!.MeetName.Should().Be("County Meet");
        result.LatestMeetImpact.MeetSlug.Should().Be("county-meet");
        result.LatestMeetImpact.TotalPerformances.Should().Be(72);
        result.LatestMeetImpact.TotalPRs.Should().Be(18);
        result.LatestMeetImpact.TotalSchoolRecords.Should().Be(2);
        result.LatestMeetImpact.TopTenAllTimeMarks.Should().Be(9);
        result.LatestMeetImpact.UniqueAthletes.Should().Be(31);
    }

    [Fact]
    public async Task GetHomePageDataAsync_LeavesLatestMeetImpactNull_WhenRepositoryReturnsNull()
    {
        SetupDefaults();
        _homeRepository.Setup(r => r.GetLatestCompletedMeetImpactAsync(1))
            .ReturnsAsync((LatestMeetImpactDto?)null);

        var result = await _service.GetHomePageDataAsync();

        result.LatestMeetImpact.Should().BeNull();
    }

    private void SetupDefaults()
    {
        _seasonRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Season>
        {
            new()
            {
                Id = 1,
                Name = "2025-2026",
                StartDate = new DateTime(2025, 12, 1),
                EndDate = new DateTime(2026, 6, 1),
                IsCurrentSeason = true
            }
        });

        _homeRepository.Setup(r => r.GetHomePageStatsAsync(1)).ReturnsAsync(new HomePageStatsDto());
        _homeRepository.Setup(r => r.GetPerformanceOnThisDayAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((OnThisDayDto?)null);
        _homeRepository.Setup(r => r.GetRecentTopPerformanceAsync(It.IsAny<int>(), It.IsAny<Environment>()))
            .ReturnsAsync((RecentHighlightDto?)null);
        _homeRepository.Setup(r => r.GetBiggestImprovementThisSeasonAsync(It.IsAny<int>(), It.IsAny<Environment>()))
            .ReturnsAsync((ImprovementDto?)null);
        _homeRepository.Setup(r => r.GetBreakoutAthleteAsync(It.IsAny<int>(), It.IsAny<Environment>()))
            .ReturnsAsync((BreakoutAthleteDto?)null);
        _homeRepository.Setup(r => r.GetSeasonLeadersAsync(It.IsAny<Gender>(), It.IsAny<int>(), It.IsAny<Environment>()))
            .ReturnsAsync(new List<SeasonLeaderDto>());
        _homeRepository.Setup(r => r.GetUpcomingMeetsAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<UpcomingMeetDto>());
    }
}
