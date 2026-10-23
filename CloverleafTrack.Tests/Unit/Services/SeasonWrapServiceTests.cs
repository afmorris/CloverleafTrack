using CloverleafTrack.DataAccess.Dtos;
using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using CloverleafTrack.Services;
using CloverleafTrack.Services.Interfaces;
using CloverleafTrack.ViewModels.Meets;
using FluentAssertions;
using Moq;

namespace CloverleafTrack.Tests.Unit.Services;

public class SeasonWrapServiceTests
{
    private static Season S(int id, string name, int year) => new() { Id = id, Name = name, StartDate = new DateTime(year, 12, 1), EndDate = new DateTime(year + 1, 6, 30) };

    private static SeasonWrapService Service(List<Season> seasons, params int[] seasonsWithMarks)
    {
        var seasonRepo = new Mock<ISeasonRepository>();
        seasonRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(seasons);
        seasonRepo.Setup(r => r.GetByNameAsync(It.IsAny<string>())).ReturnsAsync((string n) => seasons.FirstOrDefault(s => s.Name == n));

        var wrapRepo = new Mock<ISeasonWrapRepository>();
        wrapRepo.Setup(r => r.GetSeasonIdsWithMarksAsync()).ReturnsAsync(seasonsWithMarks.ToList());
        wrapRepo.Setup(r => r.GetMarksAsync(It.IsAny<int>())).ReturnsAsync(new List<SeasonWrapMarkDto>());
        wrapRepo.Setup(r => r.GetPriorBestsAsync(It.IsAny<int>(), It.IsAny<DateTime>())).ReturnsAsync(new List<SeasonWrapPriorBestDto>());
        wrapRepo.Setup(r => r.GetTeamResultsAsync(It.IsAny<int>())).ReturnsAsync(new List<SeasonWrapTeamResultDto>());

        var meets = new Mock<IMeetService>();
        meets.Setup(m => m.GetMeetScoresAsync()).ReturnsAsync(new Dictionary<int, MeetScoreViewModel>());
        return new SeasonWrapService(seasonRepo.Object, wrapRepo.Object, meets.Object, Mock.Of<IScoringService>(), Mock.Of<ISeasonSummaryRepository>());
    }

    [Fact]
    public async Task Unknown_season_is_null()
    {
        (await Service(new() { S(1, "2022-2023", 2022) }, 1).GetSeasonWrapAsync("nope")).Should().BeNull();
    }

    [Fact]
    public async Task Previous_and_next_skip_seasons_without_results()
    {
        var seasons = new List<Season> { S(1, "2021-2022", 2021), S(2, "2022-2023", 2022), S(3, "2023-2024", 2023), S(4, "2024-2025", 2024) };
        var service = Service(seasons, 1, 3, 4);   // 2022-2023 has no results

        var middle = (await service.GetSeasonWrapAsync("2023-2024"))!;
        middle.PreviousSeasonName.Should().Be("2021-2022");
        middle.NextSeasonName.Should().Be("2024-2025");

        var oldest = (await service.GetSeasonWrapAsync("2021-2022"))!;
        oldest.PreviousSeasonName.Should().BeNull();
        oldest.NextSeasonName.Should().Be("2023-2024");

        var newest = (await service.GetSeasonWrapAsync("2024-2025"))!;
        newest.NextSeasonName.Should().BeNull();
    }
}
