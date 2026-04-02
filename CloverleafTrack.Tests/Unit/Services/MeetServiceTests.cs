using CloverleafTrack.DataAccess.Dtos;
using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using CloverleafTrack.Models.Enums;
using CloverleafTrack.Services;
using FluentAssertions;
using Moq;
using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.Tests.Unit.Services;

public class MeetServiceTests
{
    private readonly Mock<IMeetRepository> _mockRepo;
    private readonly MeetService _service;

    public MeetServiceTests()
    {
        _mockRepo = new Mock<IMeetRepository>();
        _service = new MeetService(_mockRepo.Object);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static Meet BuildMeet(int id = 1, string name = "Spring Invitational") => new()
    {
        Id = id,
        Name = name,
        Date = new DateTime(2024, 3, 15),
        Environment = Environment.Outdoor,
        HandTimed = false,
        Location = new Location { Name = "Central Stadium", City = "Springfield", State = "IL" },
        Season = new Season
        {
            Id = 1, Name = "2023-2024",
            StartDate = new DateTime(2024, 1, 1),
            EndDate = new DateTime(2024, 6, 1)
        }
    };

    private static MeetPerformanceDto BuildPerf(
        int eventId,
        string eventName,
        Gender gender,
        EventCategory category = EventCategory.Sprints,
        EventType eventType = EventType.Running,
        int sortOrder = 10,
        double? timeSeconds = 11.5,
        double? distanceInches = null,
        bool pr = false,
        bool sr = false,
        string athleteName = "Jane Doe") => new()
    {
        EventId = eventId,
        EventName = eventName,
        EventGender = gender,
        EventCategory = category,
        EventType = eventType,
        EventSortOrder = sortOrder,
        TimeSeconds = timeSeconds,
        DistanceInches = distanceInches,
        PersonalBest = pr,
        SchoolRecord = sr,
        AthleteName = athleteName
    };

    private void SetupMeetDetails(Meet meet, List<MeetPerformanceDto> performances, int uniqueAthletes = 10)
    {
        _mockRepo.Setup(r => r.GetMeetBasicInfoBySlugAsync(It.IsAny<string>())).ReturnsAsync(meet);
        _mockRepo.Setup(r => r.GetPerformancesForMeetAsync(meet.Id)).ReturnsAsync(performances);
        _mockRepo.Setup(r => r.GetUniqueAthleteCountForMeetAsync(meet.Id)).ReturnsAsync(uniqueAthletes);
    }

    // -------------------------------------------------------------------------
    // GetMeetDetailsAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetMeetDetailsAsync_ReturnsNull_WhenSlugNotFound()
    {
        _mockRepo.Setup(r => r.GetMeetBasicInfoBySlugAsync(It.IsAny<string>()))
            .ReturnsAsync((Meet?)null);

        var result = await _service.GetMeetDetailsAsync("nonexistent-meet");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetMeetDetailsAsync_ReturnsMeetName()
    {
        var meet = BuildMeet(name: "Regional Championships");
        SetupMeetDetails(meet, new List<MeetPerformanceDto>());

        var result = await _service.GetMeetDetailsAsync("regional-championships");

        result!.Name.Should().Be("Regional Championships");
    }

    [Fact]
    public async Task GetMeetDetailsAsync_ReturnsLocationInfo()
    {
        var meet = BuildMeet();
        SetupMeetDetails(meet, new List<MeetPerformanceDto>());

        var result = await _service.GetMeetDetailsAsync("spring-invitational");

        result!.LocationName.Should().Be("Central Stadium");
        result.LocationCity.Should().Be("Springfield");
        result.LocationState.Should().Be("IL");
    }

    [Fact]
    public async Task GetMeetDetailsAsync_CountsPRsCorrectly()
    {
        var meet = BuildMeet();
        var performances = new List<MeetPerformanceDto>
        {
            BuildPerf(1, "100m", Gender.Male, pr: true),
            BuildPerf(2, "200m", Gender.Male, pr: false),
            BuildPerf(3, "100m", Gender.Female, pr: true),
        };
        SetupMeetDetails(meet, performances);

        var result = await _service.GetMeetDetailsAsync("spring-invitational");

        result!.TotalPRs.Should().Be(2);
    }

    [Fact]
    public async Task GetMeetDetailsAsync_CountsSchoolRecordsCorrectly()
    {
        var meet = BuildMeet();
        var performances = new List<MeetPerformanceDto>
        {
            BuildPerf(1, "100m", Gender.Male, sr: true),
            BuildPerf(2, "200m", Gender.Female, sr: true),
            BuildPerf(3, "400m", Gender.Male, sr: false),
        };
        SetupMeetDetails(meet, performances);

        var result = await _service.GetMeetDetailsAsync("spring-invitational");

        result!.TotalSchoolRecords.Should().Be(2);
    }

    [Fact]
    public async Task GetMeetDetailsAsync_SplitsBoysAndGirlsPerformances()
    {
        var meet = BuildMeet();
        var performances = new List<MeetPerformanceDto>
        {
            BuildPerf(1, "100m", Gender.Male),
            BuildPerf(2, "100m", Gender.Female),
            BuildPerf(3, "200m", Gender.Female),
        };
        SetupMeetDetails(meet, performances);

        var result = await _service.GetMeetDetailsAsync("spring-invitational");

        result!.BoysEvents.Should().HaveCount(1);   // 100m boys
        result.GirlsEvents.Should().HaveCount(2);   // 100m and 200m girls
        result.MixedEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMeetDetailsAsync_PutsMixedGenderPerformancesInMixedEvents()
    {
        var meet = BuildMeet();
        var performances = new List<MeetPerformanceDto>
        {
            BuildPerf(1, "4x400m Relay", Gender.Mixed, EventCategory.Relays, EventType.RunningRelay),
        };
        SetupMeetDetails(meet, performances);

        var result = await _service.GetMeetDetailsAsync("spring-invitational");

        result!.MixedEvents.Should().HaveCount(1);
        result.BoysEvents.Should().BeEmpty();
        result.GirlsEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMeetDetailsAsync_ReportsUniqueAthleteCount()
    {
        var meet = BuildMeet();
        SetupMeetDetails(meet, new List<MeetPerformanceDto>(), uniqueAthletes: 42);

        var result = await _service.GetMeetDetailsAsync("spring-invitational");

        result!.UniqueAthletes.Should().Be(42);
    }

    // -------------------------------------------------------------------------
    // Event ordering within BuildOrderedEventGroups
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetMeetDetailsAsync_OrdersSprints_BeforeDistance()
    {
        var meet = BuildMeet();
        var performances = new List<MeetPerformanceDto>
        {
            BuildPerf(2, "1600m", Gender.Male, EventCategory.Distance, sortOrder: 20),
            BuildPerf(1, "100m",  Gender.Male, EventCategory.Sprints,  sortOrder: 10),
        };
        SetupMeetDetails(meet, performances);

        var result = await _service.GetMeetDetailsAsync("spring-invitational");

        result!.BoysEvents[0].EventName.Should().Be("100m");
        result.BoysEvents[1].EventName.Should().Be("1600m");
    }

    [Fact]
    public async Task GetMeetDetailsAsync_PlacesRunningRelays_AfterHurdles_BeforeJumps()
    {
        var meet = BuildMeet();
        var performances = new List<MeetPerformanceDto>
        {
            BuildPerf(4, "High Jump",     Gender.Male, EventCategory.Jumps,    EventType.Field,        sortOrder: 40, timeSeconds: null, distanceInches: 72),
            BuildPerf(3, "4x100m Relay",  Gender.Male, EventCategory.Relays,   EventType.RunningRelay, sortOrder: 30),
            BuildPerf(2, "110m Hurdles",  Gender.Male, EventCategory.Hurdles,  EventType.Running,      sortOrder: 20),
            BuildPerf(1, "100m",          Gender.Male, EventCategory.Sprints,  EventType.Running,      sortOrder: 10),
        };
        SetupMeetDetails(meet, performances);

        var result = await _service.GetMeetDetailsAsync("spring-invitational");

        var names = result!.BoysEvents.Select(e => e.EventName).ToList();
        names.IndexOf("100m").Should().BeLessThan(names.IndexOf("110m Hurdles"));
        names.IndexOf("110m Hurdles").Should().BeLessThan(names.IndexOf("4x100m Relay"));
        names.IndexOf("4x100m Relay").Should().BeLessThan(names.IndexOf("High Jump"));
    }

    [Fact]
    public async Task GetMeetDetailsAsync_PlacesThrows_AfterJumps()
    {
        var meet = BuildMeet();
        var performances = new List<MeetPerformanceDto>
        {
            BuildPerf(2, "Shot Put",  Gender.Male, EventCategory.Throws, EventType.Field, sortOrder: 20, timeSeconds: null, distanceInches: 480),
            BuildPerf(1, "High Jump", Gender.Male, EventCategory.Jumps,  EventType.Field, sortOrder: 10, timeSeconds: null, distanceInches: 72),
        };
        SetupMeetDetails(meet, performances);

        var result = await _service.GetMeetDetailsAsync("spring-invitational");

        var names = result!.BoysEvents.Select(e => e.EventName).ToList();
        names.IndexOf("High Jump").Should().BeLessThan(names.IndexOf("Shot Put"));
    }

    // -------------------------------------------------------------------------
    // GetMeetsIndexAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetMeetsIndexAsync_ReturnsTotalMeetCount()
    {
        var season = new Season { Id = 1, Name = "2023-2024", StartDate = new DateTime(2024, 1, 1), EndDate = new DateTime(2024, 6, 1) };
        var location = new Location { Name = "Stadium", City = "Springfield", State = "IL" };
        var meets = new List<Meet>
        {
            new() { Id = 1, Name = "Meet A", Date = new DateTime(2024, 2, 1), SeasonId = 1, Season = season, Location = location },
            new() { Id = 2, Name = "Meet B", Date = new DateTime(2024, 3, 1), SeasonId = 1, Season = season, Location = location },
        };

        _mockRepo.Setup(r => r.GetAllMeetsWithStatsAsync()).ReturnsAsync(meets);
        _mockRepo.Setup(r => r.GetUniqueAthleteCountForMeetAsync(It.IsAny<int>())).ReturnsAsync(10);
        _mockRepo.Setup(r => r.GetPerformanceCountForMeetAsync(It.IsAny<int>())).ReturnsAsync(50);

        var result = await _service.GetMeetsIndexAsync();

        result.TotalMeets.Should().Be(2);
    }

    [Fact]
    public async Task GetMeetsIndexAsync_GroupsMeetsBySeason()
    {
        var season1 = new Season { Id = 1, Name = "2022-2023", StartDate = new DateTime(2023, 1, 1), EndDate = new DateTime(2023, 6, 1) };
        var season2 = new Season { Id = 2, Name = "2023-2024", StartDate = new DateTime(2024, 1, 1), EndDate = new DateTime(2024, 6, 1) };
        var location = new Location { Name = "Stadium", City = "Springfield", State = "IL" };
        var meets = new List<Meet>
        {
            new() { Id = 1, Name = "Old Meet",    Date = new DateTime(2023, 3, 1), SeasonId = 1, Season = season1, Location = location },
            new() { Id = 2, Name = "New Meet A",  Date = new DateTime(2024, 2, 1), SeasonId = 2, Season = season2, Location = location },
            new() { Id = 3, Name = "New Meet B",  Date = new DateTime(2024, 3, 1), SeasonId = 2, Season = season2, Location = location },
        };

        _mockRepo.Setup(r => r.GetAllMeetsWithStatsAsync()).ReturnsAsync(meets);
        _mockRepo.Setup(r => r.GetUniqueAthleteCountForMeetAsync(It.IsAny<int>())).ReturnsAsync(5);
        _mockRepo.Setup(r => r.GetPerformanceCountForMeetAsync(It.IsAny<int>())).ReturnsAsync(20);

        var result = await _service.GetMeetsIndexAsync();

        result.TotalSeasons.Should().Be(2);
        result.Seasons.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetMeetsIndexAsync_OrdersSeasons_MostRecentFirst()
    {
        var season1 = new Season { Id = 1, Name = "2022-2023", StartDate = new DateTime(2023, 1, 1), EndDate = new DateTime(2023, 6, 1) };
        var season2 = new Season { Id = 2, Name = "2023-2024", StartDate = new DateTime(2024, 1, 1), EndDate = new DateTime(2024, 6, 1) };
        var location = new Location { Name = "Stadium", City = "Springfield", State = "IL" };
        var meets = new List<Meet>
        {
            new() { Id = 1, Name = "Old Meet", Date = new DateTime(2023, 3, 1), SeasonId = 1, Season = season1, Location = location },
            new() { Id = 2, Name = "New Meet", Date = new DateTime(2024, 3, 1), SeasonId = 2, Season = season2, Location = location },
        };

        _mockRepo.Setup(r => r.GetAllMeetsWithStatsAsync()).ReturnsAsync(meets);
        _mockRepo.Setup(r => r.GetUniqueAthleteCountForMeetAsync(It.IsAny<int>())).ReturnsAsync(5);
        _mockRepo.Setup(r => r.GetPerformanceCountForMeetAsync(It.IsAny<int>())).ReturnsAsync(20);

        var result = await _service.GetMeetsIndexAsync();

        result.Seasons[0].SeasonName.Should().Be("2023-2024");
        result.Seasons[1].SeasonName.Should().Be("2022-2023");
    }

    [Fact]
    public async Task GetMeetsIndexAsync_OrdersMeetsWithinSeason_AscendingByDate()
    {
        var season = new Season { Id = 1, Name = "2023-2024", StartDate = new DateTime(2024, 1, 1), EndDate = new DateTime(2024, 6, 1) };
        var location = new Location { Name = "Stadium", City = "Springfield", State = "IL" };
        var meets = new List<Meet>
        {
            new() { Id = 2, Name = "Later Meet",   Date = new DateTime(2024, 4, 1), SeasonId = 1, Season = season, Location = location },
            new() { Id = 1, Name = "Earlier Meet", Date = new DateTime(2024, 2, 1), SeasonId = 1, Season = season, Location = location },
        };

        _mockRepo.Setup(r => r.GetAllMeetsWithStatsAsync()).ReturnsAsync(meets);
        _mockRepo.Setup(r => r.GetUniqueAthleteCountForMeetAsync(It.IsAny<int>())).ReturnsAsync(5);
        _mockRepo.Setup(r => r.GetPerformanceCountForMeetAsync(It.IsAny<int>())).ReturnsAsync(20);

        var result = await _service.GetMeetsIndexAsync();

        var meetNames = result.Seasons[0].Meets.Select(m => m.Name).ToList();
        meetNames[0].Should().Be("Earlier Meet");
        meetNames[1].Should().Be("Later Meet");
    }
}
