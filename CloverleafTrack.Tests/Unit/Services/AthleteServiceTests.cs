using CloverleafTrack.DataAccess.Dtos;
using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using CloverleafTrack.Models.Enums;
using CloverleafTrack.Models.Helpers;
using CloverleafTrack.Services;
using CloverleafTrack.ViewModels;
using FluentAssertions;
using Moq;
using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.Tests.Unit.Services;

public class AthleteServiceTests
{
    [Fact]
    public async Task GetActiveAthletesGroupedByEventCategoryAsync_OnlyReturnsActiveAthletes()
    {
        var mockRepo = new Mock<IAthleteRepository>();
        mockRepo.Setup(r => r.GetAllWithPerformancesAsync())
            .ReturnsAsync(new List<AthleteEventParticipation>
            {
                new()
                {
                    Athlete = new Athlete
                        { Id = 1, FirstName = "Jane", LastName = "Doe", IsActive = true, GraduationYear = 2025 },
                    Event = new Event { Id = 1, Name = "Discus", EventCategory = EventCategory.Throws },
                    Performance = new Performance { DistanceInches = 144 }
                },
                new()
                {
                    Athlete = new Athlete
                        { Id = 2, FirstName = "John", LastName = "Smith", IsActive = false, GraduationYear = 2023 },
                    Event = new Event { Id = 2, Name = "100m", EventCategory = EventCategory.Sprints },
                    Performance = new Performance { TimeSeconds = 11.34 }
                }
            });

        var service = new AthleteService(mockRepo.Object);
        var result = await service.GetActiveAthletesGroupedByEventCategoryAsync(currentSeason: 2024);

        result.Should().ContainKey(EventCategory.Throws);
        result.Should().NotContainKey(EventCategory.Sprints);
    }

    [Fact]
    public async Task GetActiveAthletesGroupedByEventCategoryAsync_ComputesBestDistancePR()
    {
        var mockRepo = new Mock<IAthleteRepository>();
        mockRepo.Setup(r => r.GetAllWithPerformancesAsync())
            .ReturnsAsync(new List<AthleteEventParticipation>
            {
                new()
                {
                    Athlete = new Athlete
                        { Id = 1, FirstName = "Alice", LastName = "Smith", IsActive = true, GraduationYear = 2025 },
                    Event = new Event { Id = 1, Name = "Shot Put", EventCategory = EventCategory.Throws },
                    Performance = new Performance { DistanceInches = 480 }
                },
                new()
                {
                    Athlete = new Athlete
                        { Id = 1, FirstName = "Alice", LastName = "Smith", IsActive = true, GraduationYear = 2025 },
                    Event = new Event { Id = 1, Name = "Shot Put", EventCategory = EventCategory.Throws },
                    Performance = new Performance { DistanceInches = 510 }
                }
            });

        var service = new AthleteService(mockRepo.Object);
        var result = await service.GetActiveAthletesGroupedByEventCategoryAsync(2024);

        var athlete = result[EventCategory.Throws].First();
        athlete.EventsInCategory.First().PersonalRecord.Should().Be("42' 6\"");
    }

    [Fact]
    public async Task GetFormerAthletesGroupedByGraduationYearAsync_OnlyReturnsInactiveAthletes()
    {
        var mockRepo = new Mock<IAthleteRepository>();
        mockRepo.Setup(r => r.GetAllWithPerformancesAsync())
            .ReturnsAsync(new List<AthleteEventParticipation>
            {
                new()
                {
                    Athlete = new Athlete
                        { Id = 1, FirstName = "Zoe", LastName = "Williams", IsActive = false, GraduationYear = 2022 },
                    Event = new Event { Id = 3, Name = "800m", EventCategory = EventCategory.Distance },
                    Performance = new Performance { TimeSeconds = 132.5 }
                },
                new()
                {
                    Athlete = new Athlete
                        { Id = 2, FirstName = "Mike", LastName = "Thomas", IsActive = true, GraduationYear = 2025 },
                    Event = new Event { Id = 4, Name = "High Jump", EventCategory = EventCategory.Jumps },
                    Performance = new Performance { DistanceInches = 72 }
                }
            });

        var service = new AthleteService(mockRepo.Object);
        var result = await service.GetFormerAthletesGroupedByGraduationYearAsync();

        result.Should().ContainKey(2022);
        result[2022].Should().OnlyContain(a => a.FullName == "Zoe Williams");
    }

    [Fact]
    public async Task GetFormerAthletesGroupedByGraduationYearAsync_ComputesBestTimePR()
    {
        var mockRepo = new Mock<IAthleteRepository>();
        mockRepo.Setup(r => r.GetAllWithPerformancesAsync())
            .ReturnsAsync(new List<AthleteEventParticipation>
            {
                new()
                {
                    Athlete = new Athlete
                        { Id = 3, FirstName = "Liam", LastName = "Brown", IsActive = false, GraduationYear = 2021 },
                    Event = new Event { Id = 5, Name = "1600m", EventCategory = EventCategory.Distance },
                    Performance = new Performance { TimeSeconds = 300.22 }
                },
                new()
                {
                    Athlete = new Athlete
                        { Id = 3, FirstName = "Liam", LastName = "Brown", IsActive = false, GraduationYear = 2021 },
                    Event = new Event { Id = 5, Name = "1600m", EventCategory = EventCategory.Distance },
                    Performance = new Performance { TimeSeconds = 298.75 }
                }
            });

        var service = new AthleteService(mockRepo.Object);
        var result = await service.GetFormerAthletesGroupedByGraduationYearAsync();

        var pr = result[2021].First().EventsInCategory.First().PersonalRecord;
        pr.Should().Be("4:58.75");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNullIfNotFound()
    {
        var mockRepo = new Mock<IAthleteRepository>();
        mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Athlete?)null);

        var service = new AthleteService(mockRepo.Object);
        var result = await service.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ReturnsNewId()
    {
        var mockRepo = new Mock<IAthleteRepository>();
        var vm = new AthleteViewModel { FirstName = "X", LastName = "Y", GraduationYear = 2025 };
        mockRepo.Setup(r => r.CreateAsync(It.IsAny<Athlete>())).ReturnsAsync(42);

        var service = new AthleteService(mockRepo.Object);
        var id = await service.CreateAsync(vm);

        id.Should().Be(42);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsTrue()
    {
        var mockRepo = new Mock<IAthleteRepository>();
        var vm = new AthleteViewModel { Id = 1, FirstName = "Edit", LastName = "User", GraduationYear = 2024 };
        mockRepo.Setup(r => r.UpdateAsync(It.IsAny<Athlete>())).ReturnsAsync(true);

        var service = new AthleteService(mockRepo.Object);
        var result = await service.UpdateAsync(vm);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenNotFound()
    {
        var mockRepo = new Mock<IAthleteRepository>();
        mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Athlete?)null);

        var service = new AthleteService(mockRepo.Object);
        var result = await service.DeleteAsync(1);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ReturnsTrue_WhenDeleted()
    {
        var mockRepo = new Mock<IAthleteRepository>();
        var entity = new Athlete { Id = 1, FirstName = "A", LastName = "B", GraduationYear = 2024 };
        mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(entity);
        mockRepo.Setup(r => r.DeleteAsync(entity)).ReturnsAsync(true);

        var service = new AthleteService(mockRepo.Object);
        var result = await service.DeleteAsync(1);

        result.Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // GetAthleteDetailsAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetAthleteDetailsAsync_ReturnsNull_WhenAthleteNotFound()
    {
        var mockRepo = new Mock<IAthleteRepository>();
        mockRepo.Setup(r => r.GetBySlugWithBasicInfoAsync(It.IsAny<string>()))
            .ReturnsAsync((Athlete?)null);

        var service = new AthleteService(mockRepo.Object);
        var result = await service.GetAthleteDetailsAsync("unknown-athlete", 2024);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAthleteDetailsAsync_ReturnsBasicInfo_WhenNoPerformances()
    {
        var mockRepo = new Mock<IAthleteRepository>();
        var athlete = new Athlete { Id = 1, FirstName = "Jane", LastName = "Doe", GraduationYear = 2025, Gender = Gender.Female };
        mockRepo.Setup(r => r.GetBySlugWithBasicInfoAsync("jane-doe")).ReturnsAsync(athlete);
        mockRepo.Setup(r => r.GetAllPerformancesForAthleteAsync(1)).ReturnsAsync(new List<AthletePerformanceDto>());

        var service = new AthleteService(mockRepo.Object);
        var result = await service.GetAthleteDetailsAsync("jane-doe", 2024);

        result.Should().NotBeNull();
        result!.FirstName.Should().Be("Jane");
        result.LastName.Should().Be("Doe");
    }

    [Fact]
    public async Task GetAthleteDetailsAsync_IndividualPRs_UsePersonalBestFlag()
    {
        var mockRepo = new Mock<IAthleteRepository>();
        var athlete = new Athlete { Id = 1, FirstName = "Jane", LastName = "Doe", GraduationYear = 2025, Gender = Gender.Female };
        var performances = new List<AthletePerformanceDto>
        {
            new() { EventId = 1, EventName = "100m", TimeSeconds = 11.5, PersonalBest = true,
                    MeetName = "Spring Meet", MeetDate = new DateTime(2024, 3, 1),
                    SeasonName = "2023-2024", SeasonStartDate = new DateTime(2024, 1, 1),
                    Environment = Environment.Outdoor, RelayAthletes = null },
            // Slower mark — PersonalBest = false — should NOT appear in PR table
            new() { EventId = 1, EventName = "100m", TimeSeconds = 12.0, PersonalBest = false,
                    MeetName = "Winter Meet", MeetDate = new DateTime(2024, 1, 15),
                    SeasonName = "2023-2024", SeasonStartDate = new DateTime(2024, 1, 1),
                    Environment = Environment.Outdoor, RelayAthletes = null },
        };
        mockRepo.Setup(r => r.GetBySlugWithBasicInfoAsync("jane-doe")).ReturnsAsync(athlete);
        mockRepo.Setup(r => r.GetAllPerformancesForAthleteAsync(1)).ReturnsAsync(performances);

        var service = new AthleteService(mockRepo.Object);
        var result = await service.GetAthleteDetailsAsync("jane-doe", 2024);

        result!.PersonalRecords.Should().HaveCount(1);
        result.PersonalRecords.First().Performance.Should().Be("11.50");
    }

    [Fact]
    public async Task GetAthleteDetailsAsync_RelayPRs_UseBestPerEvent_NotPersonalBestFlag()
    {
        var mockRepo = new Mock<IAthleteRepository>();
        var athlete = new Athlete { Id = 1, FirstName = "Jane", LastName = "Doe", GraduationYear = 2025, Gender = Gender.Female };
        var performances = new List<AthletePerformanceDto>
        {
            // Best relay time — PersonalBest = false (unreliable flag) — MUST still appear
            new() { EventId = 10, EventName = "4x400m Relay", TimeSeconds = 200.0, PersonalBest = false,
                    MeetName = "Spring Meet", MeetDate = new DateTime(2024, 3, 1),
                    SeasonName = "2023-2024", SeasonStartDate = new DateTime(2024, 1, 1),
                    Environment = Environment.Outdoor, RelayAthletes = "Jane Doe|~|Mary Smith|~|Lisa Jones|~|Sarah Brown" },
            // Slower relay — PersonalBest = false
            new() { EventId = 10, EventName = "4x400m Relay", TimeSeconds = 210.0, PersonalBest = false,
                    MeetName = "Winter Meet", MeetDate = new DateTime(2024, 1, 15),
                    SeasonName = "2023-2024", SeasonStartDate = new DateTime(2024, 1, 1),
                    Environment = Environment.Outdoor, RelayAthletes = "Jane Doe|~|Mary Smith|~|Lisa Jones|~|Sarah Brown" },
        };
        mockRepo.Setup(r => r.GetBySlugWithBasicInfoAsync("jane-doe")).ReturnsAsync(athlete);
        mockRepo.Setup(r => r.GetAllPerformancesForAthleteAsync(1)).ReturnsAsync(performances);

        var service = new AthleteService(mockRepo.Object);
        var result = await service.GetAthleteDetailsAsync("jane-doe", 2024);

        var relayPR = result!.PersonalRecords.FirstOrDefault(pr => pr.IsRelay);
        relayPR.Should().NotBeNull("relay PR should appear even when PersonalBest flag is false");
        relayPR!.Performance.Should().Be("3:20.00"); // 200 seconds
    }

    [Fact]
    public async Task GetAthleteDetailsAsync_TotalPRs_CountsOnlyIndividualPerformances()
    {
        var mockRepo = new Mock<IAthleteRepository>();
        var athlete = new Athlete { Id = 1, FirstName = "Jane", LastName = "Doe", GraduationYear = 2025, Gender = Gender.Female };
        var performances = new List<AthletePerformanceDto>
        {
            new() { EventId = 1, EventName = "100m", TimeSeconds = 11.5, PersonalBest = true,
                    MeetName = "Spring Meet", MeetDate = new DateTime(2024, 3, 1),
                    SeasonName = "2023-2024", SeasonStartDate = new DateTime(2024, 1, 1),
                    Environment = Environment.Outdoor, RelayAthletes = null },
            // Relay with PersonalBest = true — must NOT count toward TotalPRs
            new() { EventId = 10, EventName = "4x400m Relay", TimeSeconds = 200.0, PersonalBest = true,
                    MeetName = "Spring Meet", MeetDate = new DateTime(2024, 3, 1),
                    SeasonName = "2023-2024", SeasonStartDate = new DateTime(2024, 1, 1),
                    Environment = Environment.Outdoor, RelayAthletes = "Jane Doe|~|Mary Smith|~|Lisa Jones|~|Sarah Brown" },
        };
        mockRepo.Setup(r => r.GetBySlugWithBasicInfoAsync("jane-doe")).ReturnsAsync(athlete);
        mockRepo.Setup(r => r.GetAllPerformancesForAthleteAsync(1)).ReturnsAsync(performances);

        var service = new AthleteService(mockRepo.Object);
        var result = await service.GetAthleteDetailsAsync("jane-doe", 2024);

        result!.TotalPRs.Should().Be(1); // only the individual 100m
    }

    [Fact]
    public async Task GetAthleteDetailsAsync_TotalSchoolRecords_IncludesRelayEventsWhereAllTimeRankIsOne()
    {
        var mockRepo = new Mock<IAthleteRepository>();
        var athlete = new Athlete { Id = 1, FirstName = "Jane", LastName = "Doe", GraduationYear = 2025, Gender = Gender.Female };
        var performances = new List<AthletePerformanceDto>
        {
            // Individual school record (reliable flag)
            new() { EventId = 1, EventName = "100m", TimeSeconds = 11.5, PersonalBest = true,
                    SchoolRecord = true,
                    MeetName = "Spring Meet", MeetDate = new DateTime(2024, 3, 1),
                    SeasonName = "2023-2024", SeasonStartDate = new DateTime(2024, 1, 1),
                    Environment = Environment.Outdoor, RelayAthletes = null },
            // Relay at #1 all-time — SchoolRecord flag = false (unreliable) but AllTimeRank = 1
            new() { EventId = 10, EventName = "4x400m Relay", TimeSeconds = 200.0, PersonalBest = false,
                    SchoolRecord = false, AllTimeRank = 1,
                    MeetName = "Spring Meet", MeetDate = new DateTime(2024, 3, 1),
                    SeasonName = "2023-2024", SeasonStartDate = new DateTime(2024, 1, 1),
                    Environment = Environment.Outdoor, RelayAthletes = "Jane Doe|~|Mary Smith|~|Lisa Jones|~|Sarah Brown" },
        };
        mockRepo.Setup(r => r.GetBySlugWithBasicInfoAsync("jane-doe")).ReturnsAsync(athlete);
        mockRepo.Setup(r => r.GetAllPerformancesForAthleteAsync(1)).ReturnsAsync(performances);

        var service = new AthleteService(mockRepo.Object);
        var result = await service.GetAthleteDetailsAsync("jane-doe", 2024);

        result!.TotalSchoolRecords.Should().Be(2); // 1 individual SR + 1 relay event at rank 1
    }

    [Fact]
    public async Task GetAthleteDetailsAsync_RelayIsSchoolRecord_WhenAllTimeRankIsOne()
    {
        var mockRepo = new Mock<IAthleteRepository>();
        var athlete = new Athlete { Id = 1, FirstName = "Jane", LastName = "Doe", GraduationYear = 2025, Gender = Gender.Female };
        var performances = new List<AthletePerformanceDto>
        {
            new() { EventId = 10, EventName = "4x400m Relay", TimeSeconds = 200.0, PersonalBest = false,
                    SchoolRecord = false, AllTimeRank = 1,
                    MeetName = "Spring Meet", MeetDate = new DateTime(2024, 3, 1),
                    SeasonName = "2023-2024", SeasonStartDate = new DateTime(2024, 1, 1),
                    Environment = Environment.Outdoor, RelayAthletes = "Jane Doe|~|Mary Smith|~|Lisa Jones|~|Sarah Brown" },
        };
        mockRepo.Setup(r => r.GetBySlugWithBasicInfoAsync("jane-doe")).ReturnsAsync(athlete);
        mockRepo.Setup(r => r.GetAllPerformancesForAthleteAsync(1)).ReturnsAsync(performances);

        var service = new AthleteService(mockRepo.Object);
        var result = await service.GetAthleteDetailsAsync("jane-doe", 2024);

        var relayPR = result!.PersonalRecords.First(pr => pr.IsRelay);
        relayPR.IsSchoolRecord.Should().BeTrue();
    }

    [Fact]
    public async Task GetAthleteDetailsAsync_Seasons_OrderedMostRecentFirst()
    {
        var mockRepo = new Mock<IAthleteRepository>();
        var athlete = new Athlete { Id = 1, FirstName = "Jane", LastName = "Doe", GraduationYear = 2025, Gender = Gender.Female };
        var performances = new List<AthletePerformanceDto>
        {
            new() { EventId = 1, EventName = "100m", TimeSeconds = 11.5, PersonalBest = true,
                    MeetName = "Spring Meet", MeetDate = new DateTime(2023, 3, 1),
                    SeasonName = "2022-2023", SeasonStartDate = new DateTime(2023, 1, 1),
                    Environment = Environment.Outdoor, RelayAthletes = null },
            new() { EventId = 1, EventName = "100m", TimeSeconds = 11.2, PersonalBest = true,
                    MeetName = "Spring Meet", MeetDate = new DateTime(2024, 3, 1),
                    SeasonName = "2023-2024", SeasonStartDate = new DateTime(2024, 1, 1),
                    Environment = Environment.Outdoor, RelayAthletes = null },
        };
        mockRepo.Setup(r => r.GetBySlugWithBasicInfoAsync("jane-doe")).ReturnsAsync(athlete);
        mockRepo.Setup(r => r.GetAllPerformancesForAthleteAsync(1)).ReturnsAsync(performances);

        var service = new AthleteService(mockRepo.Object);
        var result = await service.GetAthleteDetailsAsync("jane-doe", 2024);

        result!.Seasons.Should().HaveCount(2);
        result.Seasons[0].SeasonName.Should().Be("2023-2024"); // most recent first
        result.Seasons[1].SeasonName.Should().Be("2022-2023");
    }

    [Fact]
    public async Task GetAthleteDetailsAsync_RelayMembers_ParsedFromPipeSeparatedString()
    {
        var mockRepo = new Mock<IAthleteRepository>();
        var athlete = new Athlete { Id = 1, FirstName = "Jane", LastName = "Doe", GraduationYear = 2025, Gender = Gender.Female };
        var performances = new List<AthletePerformanceDto>
        {
            new() { EventId = 10, EventName = "4x400m Relay", TimeSeconds = 200.0, PersonalBest = false,
                    MeetName = "Spring Meet", MeetDate = new DateTime(2024, 3, 1),
                    SeasonName = "2023-2024", SeasonStartDate = new DateTime(2024, 1, 1),
                    Environment = Environment.Outdoor,
                    RelayAthletes = "Jane Doe|~|Mary Smith|~|Lisa Jones|~|Sarah Brown" },
        };
        mockRepo.Setup(r => r.GetBySlugWithBasicInfoAsync("jane-doe")).ReturnsAsync(athlete);
        mockRepo.Setup(r => r.GetAllPerformancesForAthleteAsync(1)).ReturnsAsync(performances);

        var service = new AthleteService(mockRepo.Object);
        var result = await service.GetAthleteDetailsAsync("jane-doe", 2024);

        var relayPR = result!.PersonalRecords.First(pr => pr.IsRelay);
        relayPR.RelayMembers.Should().HaveCount(4);
        relayPR.RelayMembers.Should().Contain("Jane Doe");
        relayPR.RelayMembers.Should().Contain("Sarah Brown");
    }
}