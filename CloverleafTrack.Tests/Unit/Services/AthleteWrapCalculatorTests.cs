using CloverleafTrack.Models.Enums;
using CloverleafTrack.Services;
using CloverleafTrack.ViewModels.Athletes;
using FluentAssertions;
using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.Tests.Unit.Services;

public class AthleteWrapCalculatorTests
{
    private static IndividualPerformanceViewModel P(string mark, double raw, string date, string meet, bool pr = false, string? prev = null, string? relay = null) => new()
    {
        Performance = mark, RawValue = raw, Date = DateTime.Parse(date), MeetName = meet,
        IsPersonalBest = pr, PreviousBest = prev, RelayAthletes = relay
    };

    private static EventPerformanceGroupViewModel G(int id, string name, bool field, Environment env, params IndividualPerformanceViewModel[] perfs) => new()
    {
        EventId = id, EventName = name, IsFieldEvent = field, Environment = env, Performances = perfs.ToList()
    };

    private static AthleteDetailsViewModel Athlete(params SeasonPerformanceViewModel[] seasons) => new()
    {
        AthleteId = 1, FirstName = "Sam", LastName = "Stride", GraduationYear = 2026, Gender = Gender.Female, Class = "Senior",
        Seasons = seasons.Reverse().ToList()
    };

    private static SeasonPerformanceViewModel Season(string name, params EventPerformanceGroupViewModel[] groups) =>
        new() { SeasonName = name, EventGroups = groups.ToList() };

    [Fact]
    public void Counts_meets_marks_and_genuine_prs_but_not_debuts()
    {
        var a = Athlete(Season("2024-2025",
            G(1, "400M", false, Environment.Outdoor,
                P("60.00", 60, "2025-04-01", "A", pr: true),                    // debut: PR at the time but no earlier mark
                P("58.50", 58.5, "2025-05-01", "B", pr: true, prev: "60.00"))));

        var vm = AthleteWrapCalculator.Build(a, "sam-stride");

        vm.Meets.Should().Be(2);
        vm.Results.Should().Be(2);
        vm.PRs.Should().Be(1);
        vm.Seasons.Should().Be(1);
    }

    [Fact]
    public void Seasons_are_listed_oldest_first_with_the_class_of_that_season()
    {
        var a = Athlete(
            Season("2024-2025", G(1, "400M", false, Environment.Outdoor, P("60.00", 60, "2025-04-01", "A"))),
            Season("2025-2026", G(1, "400M", false, Environment.Outdoor, P("58.00", 58, "2026-04-01", "B"))));

        var vm = AthleteWrapCalculator.Build(a, "sam-stride");

        vm.FirstSeason.Should().Be("2024-2025");
        vm.LastSeason.Should().Be("2025-2026");
        vm.SeasonRows.Select(s => s.Name).Should().ContainInOrder("2024-2025", "2025-2026");
        vm.SeasonRows.Last().Class.Should().Be("Senior");
    }

    [Fact]
    public void Improvements_rank_by_relative_gain_and_handle_field_events()
    {
        var a = Athlete(Season("2025-2026",
            G(1, "400M", false, Environment.Outdoor, P("60.00", 60, "2026-03-01", "A"), P("57.00", 57, "2026-05-01", "B")),        // 5%
            G(2, "Long Jump", true, Environment.Outdoor, P("15' 0\"", 180, "2026-03-01", "A"), P("18' 0\"", 216, "2026-05-01", "B")))); // 20%

        var vm = AthleteWrapCalculator.Build(a, "sam-stride");

        vm.Improvements.Select(i => i.EventLabel).Should().ContainInOrder("Long Jump", "400M");
        vm.Improvements[0].First.Should().Be("15' 0\"");
        vm.Improvements[0].Best.Should().Be("18' 0\"");
        vm.Improvements[0].Change.Should().Be("+3' 0\"");   // longer is up
        vm.Improvements[1].Change.Should().Be("-3.00s");     // faster time is down, never "+"
    }

    [Theory]
    [InlineData(3.4, false, "-3.40s")]
    [InlineData(65.5, false, "-1:05.50")]
    [InlineData(6, true, "+6\"")]
    [InlineData(26.25, true, "+2' 2.25\"")]
    public void FormatChange_is_signed_by_direction_of_the_mark(double gain, bool isField, string expected)
    {
        AthleteWrapCalculator.FormatChange(gain, isField).Should().Be(expected);
    }

    [Fact]
    public void Relays_and_single_mark_events_have_no_improvement_row()
    {
        var a = Athlete(Season("2025-2026",
            G(1, "4x400M Relay", false, Environment.Outdoor, P("4:00.00", 240, "2026-03-01", "A", relay: "X|~|Y"), P("3:50.00", 230, "2026-05-01", "B", relay: "X|~|Y")),
            G(2, "800M", false, Environment.Outdoor, P("2:20.00", 140, "2026-05-01", "B"))));

        AthleteWrapCalculator.Build(a, "sam-stride").Improvements.Should().BeEmpty();
    }

    [Fact]
    public void Indoor_events_are_labelled_and_old_seasons_flag_partial_data()
    {
        var a = Athlete(Season("2019-2020", G(1, "800M", false, Environment.Indoor, P("2:20.00", 140, "2020-01-01", "A"), P("2:15.00", 135, "2020-02-01", "B"))));

        var vm = AthleteWrapCalculator.Build(a, "sam-stride");

        vm.HasPartialSeasons.Should().BeTrue();
        vm.Improvements.Single().EventLabel.Should().Be("800M (indoor)");
    }

    [Fact]
    public void No_results_means_no_data()
    {
        AthleteWrapCalculator.Build(Athlete(), "sam-stride").HasData.Should().BeFalse();
    }

    [Fact]
    public void Poster_is_a_one_page_pdf_for_a_long_career()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        var seasons = Enumerable.Range(2020, 6).Select(y => Season($"{y}-{y + 1}",
            Enumerable.Range(0, 8).Select(e => G(e, $"Event number {e}", e % 2 == 0, Environment.Outdoor,
                P("10.00", 10, $"{y + 1}-04-01", "A very long meet name for the invitational", pr: true, prev: "11.00"),
                P("9.50", 9.5, $"{y + 1}-05-01", "B", pr: true, prev: "10.00"))).ToArray())).ToArray();
        var vm = AthleteWrapCalculator.Build(Athlete(seasons), "sam-stride");

        var pdf = CloverleafTrack.Web.Pdf.AthleteWrapPoster.Generate(vm, "https://example.com");

        System.Text.Encoding.ASCII.GetString(pdf, 0, 5).Should().Be("%PDF-");
        CloverleafTrack.Web.Pdf.AthleteWrapPoster.PageCount(vm, "https://example.com").Should().Be(1);
    }
}
