using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using CloverleafTrack.Models.Enums;
using CloverleafTrack.Services;
using CloverleafTrack.Services.Interfaces;
using CloverleafTrack.ViewModels.Athletes;
using FluentAssertions;
using Moq;
using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.Tests.Unit.Services;

public class AthleteSummaryTests
{
    private static AthleteDetailsViewModel Details() => new()
    {
        AthleteId = 5, FirstName = "Gavin", LastName = "Forbes", GraduationYear = 2026, Gender = Gender.Male,
        TotalPRs = 4, TotalSchoolRecords = 1,
        PersonalRecords = new()
        {
            new PersonalRecordViewModel { EventId = 1, EventName = "1600M", Environment = Environment.Indoor, Performance = "4:40.71", Date = new DateTime(2026, 2, 21), MeetName = "2026 Jerome Fields HS Open", AllTimeRank = 1, IsSchoolRecord = true },
            new PersonalRecordViewModel { EventId = 2, EventName = "3200M", Environment = Environment.Outdoor, Performance = "9:43.80", Date = new DateTime(2026, 4, 24), MeetName = "2026 Keystone Invitational", AllTimeRank = 14 },
            new PersonalRecordViewModel { EventId = 3, EventName = "4x200M Relay", Environment = Environment.Indoor, Performance = "1:42.00", Date = new DateTime(2026, 2, 21), MeetName = "2026 Jerome Fields HS Open", RelayAthletes = "A B|~|Gavin Forbes" }
        },
        Seasons = new()
        {
            new SeasonPerformanceViewModel
            {
                SeasonName = "2025-2026", PRCount = 4, SchoolRecordCount = 1,
                EventGroups = new()
                {
                    new EventPerformanceGroupViewModel
                    {
                        EventId = 1, EventName = "1600M", Environment = Environment.Indoor, PersonalRecordPerformance = "4:40.71",
                        Performances = new() { new IndividualPerformanceViewModel { Performance = "4:40.71", Date = new DateTime(2026, 2, 21), MeetName = "2026 Jerome Fields HS Open", IsSchoolRecord = true, AllTimeRank = 1 } }
                    },
                    new EventPerformanceGroupViewModel
                    {
                        EventId = 2, EventName = "3200M", Environment = Environment.Outdoor, PersonalRecordPerformance = "9:43.80",
                        Performances = new() { new IndividualPerformanceViewModel { Performance = "9:43.80", Date = new DateTime(2026, 4, 24), MeetName = "2026 Keystone Invitational", AllTimeRank = 14 } }
                    }
                }
            },
            new SeasonPerformanceViewModel { SeasonName = "2022-2023", EventGroups = new() }
        }
    };

    [Fact]
    public void FactSheet_has_school_record_rank_only_when_top_ten_and_omits_unreliable_PR_counts()
    {
        var json = AthleteSummaryPrompt.BuildFactSheet(Details());

        json.Should().Contain("\"athlete\":\"Gavin Forbes\"").And.Contain("\"team\":\"boys\"").And.Contain("\"classOf\":2026");
        json.Should().Contain("\"schoolRecords\":1");
        json.Should().NotContain("personalBests").And.NotContain("allTimeRank\":14");
        json.Should().Contain("\"sr\":true").And.Contain("\"allTimeRank\":1");
    }

    [Fact]
    public void FactSheet_lists_seasons_oldest_first_with_the_class_for_that_season_and_relay_members()
    {
        var json = AthleteSummaryPrompt.BuildFactSheet(Details());

        json.IndexOf("2022-2023", StringComparison.Ordinal).Should().BeLessThan(json.IndexOf("2025-2026", StringComparison.Ordinal));
        json.Should().Contain("\"season\":\"2022-2023\",\"class\":\"Freshman\"").And.Contain("\"season\":\"2025-2026\",\"class\":\"Senior\"");
        json.Should().Contain("\"relay\":[\"A B\",\"Gavin Forbes\"]");
    }

    private static (AthleteSummaryService svc, Mock<IAthleteSummaryRepository> repo) Build(AthleteDetailsViewModel? details)
    {
        var athletes = new Mock<IAdminAthleteRepository>();
        athletes.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(new Athlete { Id = 5, FirstName = "Gavin", LastName = "Forbes", GraduationYear = 2026 });
        var athleteService = new Mock<IAthleteService>();
        athleteService.Setup(s => s.GetAthleteDetailsAsync("gavin-forbes", 2026)).ReturnsAsync(details);
        var repo = new Mock<IAthleteSummaryRepository>();
        return (new AthleteSummaryService(athletes.Object, athleteService.Object, repo.Object), repo);
    }

    [Fact]
    public async Task Service_saves_a_draft_with_the_current_hash_and_refuses_athletes_with_no_performances()
    {
        var (svc, repo) = Build(Details());
        await svc.SaveManualDraftAsync(5, "  A recap.  ", "m");
        var hash = AthleteSummaryPrompt.ComputeHash(AthleteSummaryPrompt.BuildFactSheet(Details()));
        repo.Verify(r => r.SaveDraftAsync(5, "A recap.", hash, "m"), Times.Once);

        var (empty, _) = Build(new AthleteDetailsViewModel { AthleteId = 5 });
        (await empty.GetFactSheetAsync(5)).Should().BeNull();
        var (none, _) = Build(null);
        (await none.GetFactSheetAsync(5)).Should().BeNull();
        await FluentActions.Awaiting(() => empty.SaveManualDraftAsync(5, "x", "m")).Should().ThrowAsync<InvalidOperationException>();
    }
}

public class SeniorSchoolYearTests
{
    [Fact]
    public void Senior_school_year_runs_August_to_July_and_matches_the_class_boundary()
    {
        var (start, end) = ClassYearCalculator.GetSeniorSchoolYear(2026);

        start.Should().Be(new DateTime(2025, 8, 1));
        end.Should().Be(new DateTime(2026, 8, 1));
        ClassYearCalculator.GetClassAtTimeOfPerformance(2026, start).Should().Be("Senior");
        ClassYearCalculator.GetClassAtTimeOfPerformance(2026, end.AddDays(-1)).Should().Be("Senior");
        ClassYearCalculator.GetClassAtTimeOfPerformance(2026, start.AddDays(-1)).Should().Be("Junior");
        ClassYearCalculator.GetClassAtTimeOfPerformance(2026, end).Should().NotBe("Senior");
    }
}

public class SummaryLinkifierAthleteContextTests
{
    private static readonly LinkableEvent[] Events =
    {
        new("1600M", "BOYS_OUTDOOR_1600M", Gender.Male, Environment.Outdoor),
        new("1600M", "BOYS_INDOOR_1600M", Gender.Male, Environment.Indoor),
        new("Shot Put", "BOYS_OUTDOOR_SHOT_PUT", Gender.Male, Environment.Outdoor),
    };
    private static readonly LinkableMeet[] Meets = { new("2026 Jerome Fields HS Open", "2026-jerome-fields-hs-open") };
    private static readonly LinkableAthlete[] Athletes = { new("Gavin Forbes", "gavin-forbes", Gender.Male) };

    private static string Run(string text) => SummaryLinkifier.Linkify(text, Athletes, Events, null, Meets, Gender.Male);

    [Fact]
    public void Indoor_and_outdoor_events_resolve_from_the_environment_word_and_stay_plain_without_one()
    {
        Run("He ran the indoor 1600M.").Should().Contain("boys_indoor_1600m\">1600M");
        Run("He ran the outdoor 1600M.").Should().Contain("boys_outdoor_1600m\">1600M");
        Run("He ran the 1600M.").Should().NotContain("<a ");
        Run("He threw the Shot Put.").Should().Contain("boys_outdoor_shot_put\">Shot Put"); // only one environment exists
    }

    [Fact]
    public void Meet_names_link_whole_and_the_athlete_name_links_to_the_roster()
    {
        var html = Run("Gavin Forbes ran at the 2026 Jerome Fields HS Open.");

        html.Should().Contain("href=\"/roster/gavin-forbes\">Gavin Forbes</a>").And.Contain("href=\"/meets/2026-jerome-fields-hs-open\">2026 Jerome Fields HS Open</a>");
    }
}
