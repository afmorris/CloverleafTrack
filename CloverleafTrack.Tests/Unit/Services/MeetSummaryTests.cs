using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using CloverleafTrack.Models.Enums;
using CloverleafTrack.Services;
using CloverleafTrack.Services.Interfaces;
using CloverleafTrack.ViewModels.Meets;
using FluentAssertions;
using Moq;

namespace CloverleafTrack.Tests.Unit.Services;

public class MeetSummaryTests
{
    private static MeetDetailsViewModel BuildDetails(bool relayPb = true) => new()
    {
        Name = "MAC Championships",
        Date = new DateTime(2026, 5, 9),
        TotalPerformances = 2,
        BoysEvents = new()
        {
            new MeetEventGroupViewModel
            {
                EventName = "100m",
                Performances = new()
                {
                    new MeetPerformanceViewModel { AthleteName = "Sam Runner", AthleteSlug = "sam-runner", Performance = "11.20", IsPersonalBest = true, IsSchoolRecord = true, AllTimeRank = 1 },
                    new MeetPerformanceViewModel { AthleteName = "A|~|B|~|C|~|D", AthleteSlug = null, Performance = "44.10", IsPersonalBest = relayPb, AllTimeRank = 14 }
                }
            }
        }
    };

    [Fact]
    public void FactSheet_flags_school_record_but_never_relay_pr_or_rank_beyond_ten()
    {
        var json = MeetSummaryPrompt.BuildFactSheet(BuildDetails(), MeetLevel.Varsity);

        json.Should().Contain("\"athlete\":\"Sam Runner\"").And.Contain("\"sr\":true").And.Contain("\"pr\":true");
        json.Should().Contain("\"relayTeam\":[\"A\",\"B\",\"C\",\"D\"]");
        json.Should().NotContain("allTimeRank\":14");
        // relay row must not carry a pr flag even though its PersonalBest flag was set
        json.Split("relayTeam")[1].Should().NotContain("\"pr\"");
    }

    [Fact]
    public void Format_is_open_meet_when_dual_has_no_opponents_or_results()
    {
        MeetSummaryPrompt.BuildFactSheet(BuildDetails(), MeetLevel.Varsity).Should().Contain("open meet / qualifier");
    }

    [Fact]
    public void Hash_changes_when_the_facts_change_and_is_stable_otherwise()
    {
        var a = MeetSummaryPrompt.ComputeHash(MeetSummaryPrompt.BuildFactSheet(BuildDetails(), MeetLevel.Varsity));
        var same = MeetSummaryPrompt.ComputeHash(MeetSummaryPrompt.BuildFactSheet(BuildDetails(), MeetLevel.Varsity));
        var changed = MeetSummaryPrompt.ComputeHash(MeetSummaryPrompt.BuildFactSheet(BuildDetails(), MeetLevel.JV));

        a.Should().Be(same);
        a.Should().NotBe(changed);
    }

    private static (MeetSummaryService svc, Mock<ISummaryTextGenerator> gen, Mock<IMeetSummaryRepository> repo) BuildService(MeetSummary? existing)
    {
        var meets = new Mock<IAdminMeetRepository>();
        meets.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Meet { Id = 1, Name = "MAC Championships" });
        var meetService = new Mock<IMeetService>();
        meetService.Setup(s => s.GetMeetDetailsAsync("mac-championships")).ReturnsAsync(BuildDetails());
        var repo = new Mock<IMeetSummaryRepository>();
        repo.Setup(r => r.GetForMeetAsync(1)).ReturnsAsync(existing);
        var gen = new Mock<ISummaryTextGenerator>();
        gen.Setup(g => g.GenerateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(new GeneratedText("A fine day.", "test-model"));
        return (new MeetSummaryService(meets.Object, meetService.Object, repo.Object, gen.Object), gen, repo);
    }

    [Fact]
    public async Task Generate_saves_a_draft_when_none_exists()
    {
        var (svc, gen, repo) = BuildService(null);

        (await svc.GenerateAsync(1)).Should().Be(SummaryGenerationOutcome.Generated);
        repo.Verify(r => r.SaveDraftAsync(1, "A fine day.", It.IsAny<string>(), "test-model"), Times.Once);
    }

    [Fact]
    public async Task Generate_skips_the_model_when_the_hash_is_unchanged_unless_forced()
    {
        var hash = MeetSummaryPrompt.ComputeHash(MeetSummaryPrompt.BuildFactSheet(BuildDetails(), MeetLevel.Varsity));
        var (svc, gen, _) = BuildService(new MeetSummary { MeetId = 1, InputHash = hash });

        (await svc.GenerateAsync(1)).Should().Be(SummaryGenerationOutcome.UpToDate);
        gen.Verify(g => g.GenerateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);

        (await svc.GenerateAsync(1, force: true)).Should().Be(SummaryGenerationOutcome.Generated);
    }
}
