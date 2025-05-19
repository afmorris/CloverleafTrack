using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using CloverleafTrack.Models.Enums;
using CloverleafTrack.Models.Helpers;
using CloverleafTrack.Services;
using CloverleafTrack.ViewModels;
using FluentAssertions;
using Moq;

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
}