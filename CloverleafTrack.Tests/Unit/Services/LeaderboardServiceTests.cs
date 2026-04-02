using CloverleafTrack.DataAccess.Dtos;
using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models.Enums;
using CloverleafTrack.Services;
using FluentAssertions;
using Moq;
using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.Tests.Unit.Services;

public class LeaderboardServiceTests
{
    private readonly Mock<ILeaderboardRepository> _mockRepo;
    private readonly LeaderboardService _service;

    public LeaderboardServiceTests()
    {
        _mockRepo = new Mock<ILeaderboardRepository>();
        _service = new LeaderboardService(_mockRepo.Object);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static LeaderboardDto BuildDto(
        int eventId,
        string eventName,
        string eventKey,
        Gender gender,
        Environment environment,
        EventCategory category = EventCategory.Sprints,
        EventType eventType = EventType.Running,
        int sortOrder = 10,
        double? timeSeconds = 11.5,
        double? distanceInches = null,
        string firstName = "Jane",
        string lastName = "Doe") => new()
    {
        EventId = eventId,
        EventName = eventName,
        EventKey = eventKey,
        EventCategory = category,
        EventCategorySortOrder = (int)category,
        EventSortOrder = sortOrder,
        EventType = eventType,
        Gender = gender,
        Environment = environment,
        TimeSeconds = timeSeconds,
        DistanceInches = distanceInches,
        AthleteFirstName = firstName,
        AthleteLastName = lastName,
        MeetDate = new DateTime(2024, 3, 15),
        MeetName = "Spring Invitational",
        PerformanceId = 1
    };

    // -------------------------------------------------------------------------
    // GetLeaderboardAsync — gender/environment partitioning
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetLeaderboardAsync_SeparatesBoysAndGirls()
    {
        var performances = new List<LeaderboardDto>
        {
            BuildDto(1, "100m", "100m", Gender.Male,   Environment.Outdoor),
            BuildDto(2, "100m", "100m", Gender.Female, Environment.Outdoor),
        };
        _mockRepo.Setup(r => r.GetTopPerformancePerEventAsync()).ReturnsAsync(performances);

        var result = await _service.GetLeaderboardAsync();

        result.BoysOutdoorCategories.Should().NotBeEmpty();
        result.GirlsOutdoorCategories.Should().NotBeEmpty();
        result.MixedOutdoorCategories.Should().BeEmpty();
    }

    [Fact]
    public async Task GetLeaderboardAsync_SeparatesOutdoorAndIndoor()
    {
        var performances = new List<LeaderboardDto>
        {
            BuildDto(1, "100m", "100m-outdoor", Gender.Male, Environment.Outdoor),
            BuildDto(2, "100m", "100m-indoor",  Gender.Male, Environment.Indoor),
        };
        _mockRepo.Setup(r => r.GetTopPerformancePerEventAsync()).ReturnsAsync(performances);

        var result = await _service.GetLeaderboardAsync();

        result.BoysOutdoorCategories.Should().NotBeEmpty();
        result.BoysIndoorCategories.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetLeaderboardAsync_PlacesMixedPerformances_InMixedCategories_NotBoysOrGirls()
    {
        var performances = new List<LeaderboardDto>
        {
            BuildDto(1, "4x400m Relay", "4x400m-relay", Gender.Mixed, Environment.Outdoor,
                EventCategory.Relays, EventType.RunningRelay),
        };
        _mockRepo.Setup(r => r.GetTopPerformancePerEventAsync()).ReturnsAsync(performances);

        var result = await _service.GetLeaderboardAsync();

        result.MixedOutdoorCategories.Should().NotBeEmpty();
        result.BoysOutdoorCategories.Should().BeEmpty();
        result.GirlsOutdoorCategories.Should().BeEmpty();
    }

    [Fact]
    public async Task GetLeaderboardAsync_ReturnsAllEmptyCategories_WhenNoData()
    {
        _mockRepo.Setup(r => r.GetTopPerformancePerEventAsync()).ReturnsAsync(new List<LeaderboardDto>());

        var result = await _service.GetLeaderboardAsync();

        result.BoysOutdoorCategories.Should().BeEmpty();
        result.BoysIndoorCategories.Should().BeEmpty();
        result.GirlsOutdoorCategories.Should().BeEmpty();
        result.GirlsIndoorCategories.Should().BeEmpty();
        result.MixedOutdoorCategories.Should().BeEmpty();
        result.MixedIndoorCategories.Should().BeEmpty();
    }

    // -------------------------------------------------------------------------
    // GetLeaderboardAsync — category grouping
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetLeaderboardAsync_GroupsNonRelays_ByEventCategory()
    {
        var performances = new List<LeaderboardDto>
        {
            BuildDto(1, "100m",  "100m",  Gender.Male, Environment.Outdoor, EventCategory.Sprints,  sortOrder: 10),
            BuildDto(2, "1600m", "1600m", Gender.Male, Environment.Outdoor, EventCategory.Distance, sortOrder: 20),
        };
        _mockRepo.Setup(r => r.GetTopPerformancePerEventAsync()).ReturnsAsync(performances);

        var result = await _service.GetLeaderboardAsync();

        var categoryNames = result.BoysOutdoorCategories.Select(c => c.CategoryName).ToList();
        categoryNames.Should().Contain("Sprints");
        categoryNames.Should().Contain("Distance");
    }

    [Fact]
    public async Task GetLeaderboardAsync_SeparatesRunningRelays_IntoOwnCategory()
    {
        var performances = new List<LeaderboardDto>
        {
            BuildDto(1, "100m",         "100m",         Gender.Male, Environment.Outdoor, EventCategory.Sprints, sortOrder: 10),
            BuildDto(2, "4x100m Relay", "4x100m-relay", Gender.Male, Environment.Outdoor, EventCategory.Relays, EventType.RunningRelay, sortOrder: 50),
        };
        _mockRepo.Setup(r => r.GetTopPerformancePerEventAsync()).ReturnsAsync(performances);

        var result = await _service.GetLeaderboardAsync();

        var categoryNames = result.BoysOutdoorCategories.Select(c => c.CategoryName).ToList();
        categoryNames.Should().Contain("Sprints");
        categoryNames.Should().Contain("Running Relays");
    }

    [Fact]
    public async Task GetLeaderboardAsync_SeparatesJumpRelays_IntoOwnCategory()
    {
        var performances = new List<LeaderboardDto>
        {
            BuildDto(1, "Distance Medley Jump Relay", "dmj-relay", Gender.Male, Environment.Outdoor,
                EventCategory.Relays, EventType.JumpRelay, sortOrder: 60, timeSeconds: null, distanceInches: 840),
        };
        _mockRepo.Setup(r => r.GetTopPerformancePerEventAsync()).ReturnsAsync(performances);

        var result = await _service.GetLeaderboardAsync();

        var categoryNames = result.BoysOutdoorCategories.Select(c => c.CategoryName).ToList();
        categoryNames.Should().Contain("Jump Relays");
    }

    [Fact]
    public async Task GetLeaderboardAsync_OrdersSprints_BeforeRunningRelays()
    {
        var performances = new List<LeaderboardDto>
        {
            BuildDto(2, "4x100m Relay", "4x100m-relay", Gender.Male, Environment.Outdoor, EventCategory.Relays, EventType.RunningRelay, sortOrder: 50),
            BuildDto(1, "100m",         "100m",          Gender.Male, Environment.Outdoor, EventCategory.Sprints, sortOrder: 10),
        };
        _mockRepo.Setup(r => r.GetTopPerformancePerEventAsync()).ReturnsAsync(performances);

        var result = await _service.GetLeaderboardAsync();

        var categoryNames = result.BoysOutdoorCategories.Select(c => c.CategoryName).ToList();
        categoryNames.IndexOf("Sprints").Should().BeLessThan(categoryNames.IndexOf("Running Relays"));
    }

    // -------------------------------------------------------------------------
    // GetLeaderboardDetailsAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetLeaderboardDetailsAsync_ReturnsNull_WhenNoPerformancesFound()
    {
        _mockRepo.Setup(r => r.GetAllPerformancesForEventAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<LeaderboardPerformanceDto>());

        var result = await _service.GetLeaderboardDetailsAsync("nonexistent-event");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetLeaderboardDetailsAsync_ReturnsEventInfo_FromFirstPerformance()
    {
        var performances = new List<LeaderboardPerformanceDto>
        {
            new() { PerformanceId = 1, EventId = 5, EventName = "100m", EventKey = "100m",
                    AthleteId = 1, AthleteFirstName = "Jane", AthleteLastName = "Doe",
                    TimeSeconds = 11.5, MeetName = "Spring Meet", MeetDate = new DateTime(2024, 3, 1),
                    Gender = Gender.Female, Environment = Environment.Outdoor }
        };
        _mockRepo.Setup(r => r.GetAllPerformancesForEventAsync("100m")).ReturnsAsync(performances);

        var result = await _service.GetLeaderboardDetailsAsync("100m");

        result!.EventId.Should().Be(5);
        result.EventName.Should().Be("100m");
        result.Gender.Should().Be(Gender.Female);
    }

    [Fact]
    public async Task GetLeaderboardDetailsAsync_AllPerformances_IncludesEveryRow()
    {
        var performances = new List<LeaderboardPerformanceDto>
        {
            new() { PerformanceId = 1, EventId = 1, EventName = "100m", EventKey = "100m",
                    AthleteId = 1, AthleteFirstName = "Jane", AthleteLastName = "Doe",
                    TimeSeconds = 11.5, MeetName = "Spring Meet", MeetDate = new DateTime(2024, 3, 1),
                    Gender = Gender.Female, Environment = Environment.Outdoor },
            new() { PerformanceId = 2, EventId = 1, EventName = "100m", EventKey = "100m",
                    AthleteId = 2, AthleteFirstName = "Mary", AthleteLastName = "Smith",
                    TimeSeconds = 11.8, MeetName = "Spring Meet", MeetDate = new DateTime(2024, 3, 1),
                    Gender = Gender.Female, Environment = Environment.Outdoor },
            new() { PerformanceId = 3, EventId = 1, EventName = "100m", EventKey = "100m",
                    AthleteId = 1, AthleteFirstName = "Jane", AthleteLastName = "Doe",
                    TimeSeconds = 11.9, MeetName = "Winter Meet", MeetDate = new DateTime(2024, 1, 1),
                    Gender = Gender.Female, Environment = Environment.Outdoor },
        };
        _mockRepo.Setup(r => r.GetAllPerformancesForEventAsync("100m")).ReturnsAsync(performances);

        var result = await _service.GetLeaderboardDetailsAsync("100m");

        result!.AllPerformances.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetLeaderboardDetailsAsync_PRsOnly_ShowsBestPerAthlete_ExcludingRelays()
    {
        var performances = new List<LeaderboardPerformanceDto>
        {
            // Individual athlete — two performances, first is best (query ordered)
            new() { PerformanceId = 1, EventId = 1, EventName = "100m", EventKey = "100m",
                    AthleteId = 1, AthleteFirstName = "Jane", AthleteLastName = "Doe",
                    TimeSeconds = 11.5, MeetName = "Spring Meet", MeetDate = new DateTime(2024, 3, 1),
                    Gender = Gender.Female, Environment = Environment.Outdoor },
            new() { PerformanceId = 2, EventId = 1, EventName = "100m", EventKey = "100m",
                    AthleteId = 1, AthleteFirstName = "Jane", AthleteLastName = "Doe",
                    TimeSeconds = 11.9, MeetName = "Winter Meet", MeetDate = new DateTime(2024, 1, 1),
                    Gender = Gender.Female, Environment = Environment.Outdoor },
            // Relay row — should be excluded from PRs-only list
            new() { PerformanceId = 3, EventId = 2, EventName = "4x100m Relay", EventKey = "4x100m",
                    AthleteId = null, RelayName = "Team A",
                    TimeSeconds = 48.0, MeetName = "Spring Meet", MeetDate = new DateTime(2024, 3, 1),
                    Gender = Gender.Female, Environment = Environment.Outdoor },
        };
        _mockRepo.Setup(r => r.GetAllPerformancesForEventAsync("100m")).ReturnsAsync(performances);

        var result = await _service.GetLeaderboardDetailsAsync("100m");

        result!.AllPerformances.Should().HaveCount(3);
        result.PersonalRecordsOnly.Should().HaveCount(1); // one unique athlete
        result.PersonalRecordsOnly.First().AthleteName.Should().Be("Jane Doe");
    }

    [Fact]
    public async Task GetLeaderboardDetailsAsync_DetectsRelayEvent_WhenNullAthleteIdPresent()
    {
        var performances = new List<LeaderboardPerformanceDto>
        {
            new() { PerformanceId = 1, EventId = 1, EventName = "4x400m Relay", EventKey = "4x400m-relay",
                    AthleteId = null, RelayName = "Team A",
                    TimeSeconds = 200.0, MeetName = "Spring Meet", MeetDate = new DateTime(2024, 3, 1),
                    Gender = Gender.Male, Environment = Environment.Outdoor },
        };
        _mockRepo.Setup(r => r.GetAllPerformancesForEventAsync("4x400m-relay")).ReturnsAsync(performances);

        var result = await _service.GetLeaderboardDetailsAsync("4x400m-relay");

        result!.IsRelayEvent.Should().BeTrue();
    }

    [Fact]
    public async Task GetLeaderboardDetailsAsync_IsNotRelayEvent_WhenAllPerformancesHaveAthleteId()
    {
        var performances = new List<LeaderboardPerformanceDto>
        {
            new() { PerformanceId = 1, EventId = 1, EventName = "100m", EventKey = "100m",
                    AthleteId = 1, AthleteFirstName = "Jane", AthleteLastName = "Doe",
                    TimeSeconds = 11.5, MeetName = "Spring Meet", MeetDate = new DateTime(2024, 3, 1),
                    Gender = Gender.Female, Environment = Environment.Outdoor },
        };
        _mockRepo.Setup(r => r.GetAllPerformancesForEventAsync("100m")).ReturnsAsync(performances);

        var result = await _service.GetLeaderboardDetailsAsync("100m");

        result!.IsRelayEvent.Should().BeFalse();
    }
}
