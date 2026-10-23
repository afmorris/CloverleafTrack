using CloverleafTrack.DataAccess.Dtos;
using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using CloverleafTrack.Models.Enums;
using CloverleafTrack.Services;
using CloverleafTrack.Services.Interfaces;
using CloverleafTrack.ViewModels.Seasons;
using FluentAssertions;
using Moq;
using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.Tests.Unit.Services;

public class SeasonSummaryTests
{
    private static SeasonWrapViewModel Wrap(string? summary = "Existing text") => new()
    {
        SeasonId = 7, Name = "2025-2026", Meets = 10, Results = 400, AthletesCompeted = 60, PRs = 120, AthletesWithPRs = 40,
        SchoolRecordsSet = 1, SchoolRecordBreaks = 2, TopTenMarks = 9, SummaryText = summary,
        SchoolRecords =
        {
            new WrapSchoolRecord
            {
                EventLabel = "100M", Gender = Gender.Male, Environment = Environment.Outdoor, Holders = { new WrapPerson { Name = "Sam Runner", Slug = "sam-runner" } },
                Mark = "10.89", MeetName = "MAC Championships", MeetSlug = "mac", Date = new DateTime(2026, 5, 9), TimesBroken = 2, StillRecord = true
            }
        },
        Standouts = { new WrapStandout { Athlete = new WrapPerson { Name = "Ann Lee", Slug = "ann-lee" }, EventLabel = "Shot Put", Mark = "40' 2\"", AllTimeRank = 14, MeetName = "Meet A", Date = new DateTime(2026, 4, 8) } }
    };

    [Fact]
    public void FactSheet_has_names_marks_counts_and_data_rule_notes_but_no_slugs_or_summary()
    {
        var json = SeasonSummaryPrompt.BuildFactSheet(Wrap());

        json.Should().Contain("\"season\":\"2025-2026\"").And.Contain("\"personalRecords\":120").And.Contain("\"athlete\":\"Ann Lee\"");
        json.Should().Contain("\"mark\":\"10.89\"").And.Contain("\"holders\":[\"Sam Runner\"]").And.Contain("first-ever mark");
        json.Should().NotContain("sam-runner").And.NotContain("Existing text");
    }

    [Fact]
    public void FactSheet_omits_rank_beyond_ten_and_absent_sections()
    {
        var json = SeasonSummaryPrompt.BuildFactSheet(Wrap());

        json.Should().NotContain("allTimeRank").And.NotContain("dualRecord").And.NotContain("scoringLeaders").And.NotContain("inProgress");
    }

    [Fact]
    public void Hash_changes_with_the_fact_sheet()
    {
        var a = SeasonSummaryPrompt.ComputeHash(SeasonSummaryPrompt.BuildFactSheet(Wrap()));
        var w = Wrap(); w.PRs = 121;
        var b = SeasonSummaryPrompt.ComputeHash(SeasonSummaryPrompt.BuildFactSheet(w));
        a.Should().NotBe(b);
    }

    [Fact]
    public async Task SaveManualDraft_stores_trimmed_text_with_the_current_hash()
    {
        var seasons = new Mock<IAdminSeasonRepository>();
        seasons.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(new Season { Id = 7, Name = "2025-2026" });
        var wrap = new Mock<ISeasonWrapService>();
        wrap.Setup(w => w.GetSeasonWrapAsync("2025-2026")).ReturnsAsync(Wrap());
        var repo = new Mock<ISeasonSummaryRepository>();
        var service = new SeasonSummaryService(seasons.Object, wrap.Object, repo.Object);

        await service.SaveManualDraftAsync(7, "  Text.  ", "opus");

        var expected = SeasonSummaryPrompt.ComputeHash(SeasonSummaryPrompt.BuildFactSheet(Wrap()));
        repo.Verify(r => r.SaveDraftAsync(7, "Text.", expected, "opus"), Times.Once);
    }

    [Fact]
    public async Task SaveManualDraft_throws_when_the_season_has_no_results()
    {
        var seasons = new Mock<IAdminSeasonRepository>();
        seasons.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(new Season { Id = 7, Name = "2025-2026" });
        var wrap = new Mock<ISeasonWrapService>();
        wrap.Setup(w => w.GetSeasonWrapAsync("2025-2026")).ReturnsAsync(new SeasonWrapViewModel { Name = "2025-2026" });
        var service = new SeasonSummaryService(seasons.Object, wrap.Object, new Mock<ISeasonSummaryRepository>().Object);

        var act = () => service.SaveManualDraftAsync(7, "x", "opus");

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task WrapService_attaches_only_the_approved_text()
    {
        var season = new Season { Id = 7, Name = "2025-2026", StartDate = new DateTime(2025, 12, 1), EndDate = new DateTime(2026, 6, 30) };
        var seasons = new Mock<ISeasonRepository>();
        seasons.Setup(r => r.GetByNameAsync("2025-2026")).ReturnsAsync(season);
        seasons.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Season> { season });
        var wrapRepo = new Mock<ISeasonWrapRepository>();
        wrapRepo.Setup(r => r.GetMarksAsync(7)).ReturnsAsync(new List<SeasonWrapMarkDto>
        {
            new() { PerformanceId = 1, AthleteId = 1, AthleteFirstName = "A", AthleteLastName = "B", EventId = 1, EventName = "100M", EventType = EventType.Running,
                    TimeSeconds = 12, MeetId = 1, MeetName = "M", MeetDate = new DateTime(2026, 4, 8) }
        });
        wrapRepo.Setup(r => r.GetSeasonIdsWithMarksAsync()).ReturnsAsync(new List<int> { 7 });
        wrapRepo.Setup(r => r.GetPriorBestsAsync(7, It.IsAny<DateTime>())).ReturnsAsync(new List<SeasonWrapPriorBestDto>());
        wrapRepo.Setup(r => r.GetTeamResultsAsync(7)).ReturnsAsync(new List<SeasonWrapTeamResultDto>());
        var meets = new Mock<IMeetService>();
        meets.Setup(m => m.GetMeetScoresAsync()).ReturnsAsync(new Dictionary<int, CloverleafTrack.ViewModels.Meets.MeetScoreViewModel>());
        var summaries = new Mock<ISeasonSummaryRepository>();
        summaries.Setup(s => s.GetApprovedTextAsync(7)).ReturnsAsync("Approved words.");
        var service = new SeasonWrapService(seasons.Object, wrapRepo.Object, meets.Object, new Mock<IScoringService>().Object, summaries.Object);

        var vm = await service.GetSeasonWrapAsync("2025-2026");

        vm!.SummaryText.Should().Be("Approved words.");
    }
}
