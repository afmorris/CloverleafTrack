using CloverleafTrack.DataAccess.Dtos;
using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using CloverleafTrack.Models.Enums;
using CloverleafTrack.Services;
using FluentAssertions;
using Moq;
using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.Tests.Unit.Services;

/// <summary>Public meet page shows a summary only when a reviewer approved it.</summary>
public class MeetServiceSummaryTests
{
    private static MeetService Build(MeetSummary? summary, bool withSummaryRepo = true)
    {
        var meet = new Meet
        {
            Id = 7, Name = "Spring Invitational", Date = new DateTime(2026, 4, 1), Environment = Environment.Outdoor,
            Location = new Location { Name = "Stadium" },
            Season = new Season { Id = 1, Name = "2025-2026", StartDate = new DateTime(2025, 12, 1), EndDate = new DateTime(2026, 6, 30) }
        };
        var perfs = new List<MeetPerformanceDto>
        {
            new()
            {
                PerformanceId = 1, EventId = 1, EventName = "100M", EventKey = "BOYS_OUTDOOR_100M", EventGender = Gender.Male,
                EventCategory = EventCategory.Sprints, EventType = EventType.Running, AthleteId = 5, AthleteName = "Grady Pamer", TimeSeconds = 11.9
            }
        };
        var repo = new Mock<IMeetRepository>();
        repo.Setup(r => r.GetMeetBasicInfoBySlugAsync(It.IsAny<string>())).ReturnsAsync(meet);
        repo.Setup(r => r.GetPerformancesForMeetAsync(7)).ReturnsAsync(perfs);
        repo.Setup(r => r.GetUniqueAthleteCountForMeetAsync(7)).ReturnsAsync(1);
        repo.Setup(r => r.GetParticipantsForMeetAsync(7)).ReturnsAsync(new List<MeetParticipant>());
        repo.Setup(r => r.GetTeamResultsForMeetsAsync(It.IsAny<List<int>>())).ReturnsAsync(new List<MeetTeamResult>());
        var placings = new Mock<IMeetPlacingRepository>();
        placings.Setup(r => r.GetForMeetAsync(It.IsAny<int>())).ReturnsAsync(new List<MeetPlacing>());
        var summaries = new Mock<IMeetSummaryRepository>();
        summaries.Setup(r => r.GetForMeetAsync(7)).ReturnsAsync(summary);
        return new MeetService(repo.Object, placings.Object, null, withSummaryRepo ? summaries.Object : null);
    }

    [Fact]
    public async Task Approved_summary_is_shown_with_athlete_and_event_links()
    {
        var svc = Build(new MeetSummary { MeetId = 7, Status = SummaryStatus.Approved, SummaryText = "Grady Pamer won the boys 100M." });

        var vm = await svc.GetMeetDetailsAsync("spring-invitational");

        vm!.SummaryText.Should().Be("Grady Pamer won the boys 100M.");
        vm.SummaryHtml.Should().Contain("href=\"/roster/grady-pamer\"").And.Contain("href=\"/events/boys_outdoor_100m\"")
            .And.Contain("hover:underline");
    }

    [Fact]
    public async Task Draft_summary_never_reaches_the_public_page()
    {
        var svc = Build(new MeetSummary { MeetId = 7, Status = SummaryStatus.Draft, SummaryText = "Unreviewed text." });

        var vm = await svc.GetMeetDetailsAsync("spring-invitational");

        vm!.SummaryText.Should().BeNull();
        vm.SummaryHtml.Should().BeNull();
    }

    [Fact]
    public async Task Meet_without_a_summary_or_repository_renders_as_before()
    {
        (await Build(null).GetMeetDetailsAsync("x"))!.SummaryHtml.Should().BeNull();
        (await Build(null, withSummaryRepo: false).GetMeetDetailsAsync("x"))!.SummaryHtml.Should().BeNull();
    }
}
