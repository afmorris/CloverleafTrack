using CloverleafTrack.DataAccess.Dtos;
using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using CloverleafTrack.Models.Enums;
using CloverleafTrack.Services;
using FluentAssertions;
using Moq;
using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.Tests.Unit.Services;

/// <summary>The public athlete page shows a summary only when a reviewer approved it.</summary>
public class AthleteServiceSummaryTests
{
    private static AthleteService Build(AthleteSummary? summary, bool withSummaryRepo = true)
    {
        var athletes = new Mock<IAthleteRepository>();
        athletes.Setup(r => r.GetBySlugWithBasicInfoAsync("jane-doe"))
            .ReturnsAsync(new Athlete { Id = 1, FirstName = "Jane", LastName = "Doe", GraduationYear = 2025, Gender = Gender.Female });
        athletes.Setup(r => r.GetAllPerformancesForAthleteAsync(1)).ReturnsAsync(new List<AthletePerformanceDto>
        {
            new() { EventId = 7, EventName = "100M", TimeSeconds = 11.5, PersonalBest = true, MeetName = "2024 Spring Meet",
                    MeetDate = new DateTime(2024, 3, 1), SeasonName = "2023-2024", SeasonStartDate = new DateTime(2024, 1, 1),
                    Environment = Environment.Outdoor }
        });
        var summaries = new Mock<IAthleteSummaryRepository>();
        summaries.Setup(r => r.GetForAthleteAsync(1)).ReturnsAsync(summary);
        var events = new Mock<IAdminEventRepository>();
        events.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Event>
        {
            new() { Id = 7, Name = "100M", EventKey = "GIRLS_OUTDOOR_100M", Gender = Gender.Female, Environment = Environment.Outdoor }
        });
        return new AthleteService(athletes.Object, null, withSummaryRepo ? summaries.Object : null, new AthleteSummaryLinker(events.Object));
    }

    [Fact]
    public async Task Approved_summary_is_shown_with_meet_and_event_links()
    {
        var svc = Build(new AthleteSummary { AthleteId = 1, Status = SummaryStatus.Approved, SummaryText = "Jane Doe ran the 100M at the 2024 Spring Meet." });

        var vm = await svc.GetAthleteDetailsAsync("jane-doe", 2025);

        vm!.SummaryText.Should().Be("Jane Doe ran the 100M at the 2024 Spring Meet.");
        vm.SummaryHtml.Should().Contain("href=\"/roster/jane-doe\"").And.Contain("href=\"/events/girls_outdoor_100m\"")
            .And.Contain("href=\"/meets/2024-spring-meet\"").And.Contain("hover:underline");
    }

    [Fact]
    public async Task Draft_summary_never_reaches_the_public_athlete_page()
    {
        var vm = await Build(new AthleteSummary { AthleteId = 1, Status = SummaryStatus.Draft, SummaryText = "Unreviewed." })
            .GetAthleteDetailsAsync("jane-doe", 2025);

        vm!.SummaryText.Should().BeNull();
        vm.SummaryHtml.Should().BeNull();
    }

    [Fact]
    public async Task Athlete_without_a_summary_or_repository_renders_as_before()
    {
        (await Build(null).GetAthleteDetailsAsync("jane-doe", 2025))!.SummaryHtml.Should().BeNull();
        (await Build(null, withSummaryRepo: false).GetAthleteDetailsAsync("jane-doe", 2025))!.SummaryHtml.Should().BeNull();
    }
}
