using CloverleafTrack.DataAccess.Dtos;
using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using CloverleafTrack.Models.Enums;
using CloverleafTrack.Services;
using FluentAssertions;
using Moq;
using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.Tests.Unit.Services;

public class LeaderboardServiceTests
{
    private readonly Mock<ILeaderboardRepository> _mockRepo;
    private readonly Mock<ISeasonRepository> _mockSeasonRepo;
    private readonly LeaderboardService _service;

    public LeaderboardServiceTests()
    {
        _mockRepo = new Mock<ILeaderboardRepository>();
        _mockSeasonRepo = new Mock<ISeasonRepository>();
        // Default: no seasons configured — scope resolution falls back to all-time behavior
        // and SeasonOptions/CurrentSeasonId stay empty/null unless a test overrides this.
        _mockSeasonRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Season>());
        _service = new LeaderboardService(_mockRepo.Object, _mockSeasonRepo.Object);
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

        result.BoysOutdoor.Should().NotBeEmpty();
        result.GirlsOutdoor.Should().NotBeEmpty();
        result.MixedOutdoor.Should().BeEmpty();
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

        result.BoysOutdoor.Should().NotBeEmpty();
        result.BoysIndoor.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetLeaderboardAsync_PlacesMixedPerformances_InMixedList_NotBoysOrGirls()
    {
        var performances = new List<LeaderboardDto>
        {
            BuildDto(1, "4x400m Relay", "4x400m-relay", Gender.Mixed, Environment.Outdoor,
                EventCategory.Relays, EventType.RunningRelay),
        };
        _mockRepo.Setup(r => r.GetTopPerformancePerEventAsync()).ReturnsAsync(performances);

        var result = await _service.GetLeaderboardAsync();

        result.MixedOutdoor.Should().NotBeEmpty();
        result.BoysOutdoor.Should().BeEmpty();
        result.GirlsOutdoor.Should().BeEmpty();
    }

    [Fact]
    public async Task GetLeaderboardAsync_ReturnsAllEmpty_WhenNoData()
    {
        _mockRepo.Setup(r => r.GetTopPerformancePerEventAsync()).ReturnsAsync(new List<LeaderboardDto>());

        var result = await _service.GetLeaderboardAsync();

        result.BoysOutdoor.Should().BeEmpty();
        result.BoysIndoor.Should().BeEmpty();
        result.GirlsOutdoor.Should().BeEmpty();
        result.GirlsIndoor.Should().BeEmpty();
        result.MixedOutdoor.Should().BeEmpty();
        result.MixedIndoor.Should().BeEmpty();
    }

    [Fact]
    public async Task GetLeaderboardAsync_OrdersEvents_BySortOrder()
    {
        var performances = new List<LeaderboardDto>
        {
            BuildDto(2, "1600m", "1600m", Gender.Male, Environment.Outdoor, sortOrder: 20),
            BuildDto(1, "100m",  "100m",  Gender.Male, Environment.Outdoor, sortOrder: 10),
            BuildDto(3, "Shot Put", "shot-put", Gender.Male, Environment.Outdoor, sortOrder: 30,
                timeSeconds: null, distanceInches: 480),
        };
        _mockRepo.Setup(r => r.GetTopPerformancePerEventAsync()).ReturnsAsync(performances);

        var result = await _service.GetLeaderboardAsync();

        var eventNames = result.BoysOutdoor.Select(e => e.EventName).ToList();
        eventNames.Should().ContainInOrder("100m", "1600m", "Shot Put");
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

    // -------------------------------------------------------------------------
    // Scope / Depth / Class composition (Issue #3 — Event page depth)
    // -------------------------------------------------------------------------

    private static LeaderboardPerformanceDto BuildPerfDto(
        int performanceId, int athleteId, string firstName, string lastName,
        int graduationYear, double timeSeconds, DateTime meetDate,
        string meetName = "Meet", int? allTimeRank = null,
        string eventKey = "100m", string eventName = "100m") => new()
    {
        PerformanceId = performanceId,
        EventId = 1,
        EventName = eventName,
        EventKey = eventKey,
        Gender = Gender.Male,
        Environment = Environment.Outdoor,
        AthleteId = athleteId,
        AthleteFirstName = firstName,
        AthleteLastName = lastName,
        GraduationYear = graduationYear,
        TimeSeconds = timeSeconds,
        MeetName = meetName,
        MeetDate = meetDate,
        AllTimeRank = allTimeRank,
    };

    [Fact]
    public async Task GetLeaderboardDetailsAsync_DefaultDepth_Is25()
    {
        var performances = Enumerable.Range(1, 30)
            .Select(i => BuildPerfDto(i, i, "Athlete", $"{i}", 2024, 10.0 + i, new DateTime(2024, 3, 1)))
            .ToList();
        _mockRepo.Setup(r => r.GetAllPerformancesForEventAsync("100m", null)).ReturnsAsync(performances);

        var result = await _service.GetLeaderboardDetailsAsync("100m");

        result!.DepthValue.Should().Be(25);
        result.AllPerformances.Should().HaveCount(25);
        result.TotalPerformanceCount.Should().Be(30);
    }

    [Fact]
    public async Task GetLeaderboardDetailsAsync_Depth0_MeansAll_NoTruncation()
    {
        var performances = Enumerable.Range(1, 30)
            .Select(i => BuildPerfDto(i, i, "Athlete", $"{i}", 2024, 10.0 + i, new DateTime(2024, 3, 1)))
            .ToList();
        _mockRepo.Setup(r => r.GetAllPerformancesForEventAsync("100m", null)).ReturnsAsync(performances);

        var result = await _service.GetLeaderboardDetailsAsync("100m", depth: 0);

        result!.DepthValue.Should().Be(0);
        result.AllPerformances.Should().HaveCount(30);
    }

    [Fact]
    public async Task GetLeaderboardDetailsAsync_Depth_AppliesPerClass_NotSlicedFromOverallTopN()
    {
        // 3 seniors are the fastest performances overall.
        // 3 juniors are much slower — they would be entirely excluded if depth were applied to
        // the overall list BEFORE class filtering. Composition rule: depth applies AFTER scope
        // and class filtering, so "Top 2 Juniors" must still return the 2 fastest juniors.
        var performances = new List<LeaderboardPerformanceDto>
        {
            BuildPerfDto(1, 1, "Sam", "Senior1", 2024, 10.0, new DateTime(2024, 3, 1)),
            BuildPerfDto(2, 2, "Sam", "Senior2", 2024, 10.1, new DateTime(2024, 3, 1)),
            BuildPerfDto(3, 3, "Sam", "Senior3", 2024, 10.2, new DateTime(2024, 3, 1)),
            BuildPerfDto(4, 4, "Jun", "Junior1", 2025, 20.0, new DateTime(2024, 3, 1)),
            BuildPerfDto(5, 5, "Jun", "Junior2", 2025, 20.1, new DateTime(2024, 3, 1)),
            BuildPerfDto(6, 6, "Jun", "Junior3", 2025, 20.2, new DateTime(2024, 3, 1)),
        };
        _mockRepo.Setup(r => r.GetAllPerformancesForEventAsync("100m", null)).ReturnsAsync(performances);

        var result = await _service.GetLeaderboardDetailsAsync("100m", depth: 2);

        result!.ClassAllPerformances["Junior"].Should().HaveCount(2);
        result.ClassAllPerformances["Junior"].Select(p => p.AthleteName)
            .Should().BeEquivalentTo(new[] { "Jun Junior1", "Jun Junior2" });

        // Rank renumbers to 1..N *within the filtered (class + depth) set*.
        result.ClassAllPerformances["Junior"][0].Rank.Should().Be(1);
        result.ClassAllPerformances["Junior"][1].Rank.Should().Be(2);

        // The overall "all classes" list is depth-limited independently and is unaffected by the
        // per-class lists — it's the two fastest performances overall (both seniors).
        result.AllPerformances.Should().HaveCount(2);
        result.AllPerformances.Select(p => p.AthleteName)
            .Should().BeEquivalentTo(new[] { "Sam Senior1", "Sam Senior2" });
    }

    [Fact]
    public async Task GetLeaderboardDetailsAsync_Depth_AppliesToPersonalRecordsPerClassToo()
    {
        var performances = new List<LeaderboardPerformanceDto>
        {
            BuildPerfDto(1, 1, "A", "Junior1", 2025, 20.0, new DateTime(2024, 3, 1)),
            BuildPerfDto(2, 2, "B", "Junior2", 2025, 20.1, new DateTime(2024, 3, 1)),
            BuildPerfDto(3, 3, "C", "Junior3", 2025, 20.2, new DateTime(2024, 3, 1)),
        };
        _mockRepo.Setup(r => r.GetAllPerformancesForEventAsync("100m", null)).ReturnsAsync(performances);

        var result = await _service.GetLeaderboardDetailsAsync("100m", depth: 2);

        result!.ClassPersonalRecords["Junior"].Should().HaveCount(2);
        result.PersonalRecordsOnly.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetLeaderboardDetailsAsync_Scope_AllTime_PassesNullSeasonIdToRepository()
    {
        var performances = new List<LeaderboardPerformanceDto> { BuildPerfDto(1, 1, "A", "B", 2024, 10.0, new DateTime(2024, 3, 1)) };
        _mockRepo.Setup(r => r.GetAllPerformancesForEventAsync("100m", null)).ReturnsAsync(performances);

        var result = await _service.GetLeaderboardDetailsAsync("100m", scope: "all-time");

        result!.ScopeValue.Should().Be("all-time");
        _mockRepo.Verify(r => r.GetAllPerformancesForEventAsync("100m", null), Times.Once);
    }

    [Fact]
    public async Task GetLeaderboardDetailsAsync_Scope_Season_ResolvesToCurrentSeasonId()
    {
        _mockSeasonRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Season>
        {
            new() { Id = 7, Name = "2026 Outdoor", IsCurrentSeason = true, StartDate = new DateTime(2026, 3, 1), EndDate = new DateTime(2026, 6, 1) },
            new() { Id = 5, Name = "2019 Outdoor", IsCurrentSeason = false, StartDate = new DateTime(2019, 3, 1), EndDate = new DateTime(2019, 6, 1) },
        });
        var performances = new List<LeaderboardPerformanceDto> { BuildPerfDto(1, 1, "A", "B", 2024, 10.0, new DateTime(2024, 3, 1)) };
        _mockRepo.Setup(r => r.GetAllPerformancesForEventAsync("100m", 7)).ReturnsAsync(performances);

        var result = await _service.GetLeaderboardDetailsAsync("100m", scope: "season");

        result!.ScopeValue.Should().Be("season");
        result.CurrentSeasonId.Should().Be(7);
        _mockRepo.Verify(r => r.GetAllPerformancesForEventAsync("100m", 7), Times.Once);
    }

    [Fact]
    public async Task GetLeaderboardDetailsAsync_Scope_SpecificPastSeason_ResolvesSeasonId()
    {
        _mockSeasonRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Season>
        {
            new() { Id = 5, Name = "2019 Outdoor", IsCurrentSeason = false, StartDate = new DateTime(2019, 3, 1), EndDate = new DateTime(2019, 6, 1) },
        });
        var performances = new List<LeaderboardPerformanceDto> { BuildPerfDto(1, 1, "A", "B", 2019, 10.0, new DateTime(2019, 3, 1)) };
        _mockRepo.Setup(r => r.GetAllPerformancesForEventAsync("100m", 5)).ReturnsAsync(performances);

        var result = await _service.GetLeaderboardDetailsAsync("100m", scope: "season-5");

        result!.ScopeValue.Should().Be("season-5");
        _mockRepo.Verify(r => r.GetAllPerformancesForEventAsync("100m", 5), Times.Once);
    }

    [Fact]
    public async Task GetLeaderboardDetailsAsync_SeasonOptions_ExcludesCurrentSeason_OrderedMostRecentFirst()
    {
        _mockSeasonRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Season>
        {
            new() { Id = 7, Name = "2026 Outdoor", IsCurrentSeason = true, StartDate = new DateTime(2026, 3, 1) },
            new() { Id = 5, Name = "2019 Outdoor", IsCurrentSeason = false, StartDate = new DateTime(2019, 3, 1) },
            new() { Id = 6, Name = "2022 Outdoor", IsCurrentSeason = false, StartDate = new DateTime(2022, 3, 1) },
        });
        var performances = new List<LeaderboardPerformanceDto> { BuildPerfDto(1, 1, "A", "B", 2024, 10.0, new DateTime(2024, 3, 1)) };
        _mockRepo.Setup(r => r.GetAllPerformancesForEventAsync("100m", null)).ReturnsAsync(performances);

        var result = await _service.GetLeaderboardDetailsAsync("100m");

        result!.SeasonOptions.Select(s => s.Id).Should().ContainInOrder(6, 5);
        result.SeasonOptions.Should().NotContain(s => s.Id == 7);
    }

    [Fact]
    public async Task GetLeaderboardDetailsAsync_EmptyScopedSeason_ReturnsEmptyLists_NotNull_WhenEventExistsAllTime()
    {
        // The requested season has no performances for this event, but the event has been run
        // in other seasons — this must render an empty-state page, not 404.
        _mockRepo.Setup(r => r.GetAllPerformancesForEventAsync("100m", 5)).ReturnsAsync(new List<LeaderboardPerformanceDto>());
        var allTime = new List<LeaderboardPerformanceDto> { BuildPerfDto(1, 1, "A", "B", 2019, 10.0, new DateTime(2019, 3, 1)) };
        _mockRepo.Setup(r => r.GetAllPerformancesForEventAsync("100m", null)).ReturnsAsync(allTime);

        var result = await _service.GetLeaderboardDetailsAsync("100m", scope: "season-5");

        result.Should().NotBeNull();
        result!.AllPerformances.Should().BeEmpty();
        result.TotalPerformanceCount.Should().Be(0);
        result.EventName.Should().Be("100m"); // metadata falls back to the all-time query
    }

    [Fact]
    public async Task GetLeaderboardDetailsAsync_ReturnsNull_WhenEventHasNoPerformancesEverEvenAllTime()
    {
        _mockRepo.Setup(r => r.GetAllPerformancesForEventAsync("nonexistent", null)).ReturnsAsync(new List<LeaderboardPerformanceDto>());

        var result = await _service.GetLeaderboardDetailsAsync("nonexistent");

        result.Should().BeNull();
    }
}
