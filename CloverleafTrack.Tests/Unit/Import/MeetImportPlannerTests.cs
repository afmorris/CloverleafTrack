using CloverleafTrack.DataAccess.Dtos;
using CloverleafTrack.Models;
using CloverleafTrack.Models.Enums;
using CloverleafTrack.Web.Import;
using FluentAssertions;
using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.Tests.Unit.Import;

public class MeetImportPlannerTests
{
    private static MeetImportContext Context(MeetType type = MeetType.Dual, params string[] opponents)
    {
        var ctx = new MeetImportContext
        {
            Meet = new Meet { Id = 7, Name = "2026 Test Dual", Date = new DateTime(2026, 4, 14), Environment = Environment.Outdoor, MeetType = type },
            Events = new List<Event>
            {
                new() { Id = 1, Name = "100M", Gender = Gender.Male, EventType = EventType.Running, AthleteCount = 1 },
                new() { Id = 2, Name = "100M", Gender = Gender.Female, EventType = EventType.Running, AthleteCount = 1 },
                new() { Id = 3, Name = "Shot Put", Gender = Gender.Male, EventType = EventType.Field, AthleteCount = 1 },
                new() { Id = 4, Name = "4x100M Relay", Gender = Gender.Male, EventType = EventType.RunningRelay, AthleteCount = 4 },
                new() { Id = 5, Name = "800M", Gender = Gender.Male, EventType = EventType.Running, AthleteCount = 1 },
                new() { Id = 6, Name = "1600M", Gender = Gender.Male, EventType = EventType.Running, AthleteCount = 1 },
                new() { Id = 7, Name = "3200M", Gender = Gender.Male, EventType = EventType.Running, AthleteCount = 1 },
                new() { Id = 8, Name = "Long Jump", Gender = Gender.Male, EventType = EventType.Field, AthleteCount = 1 },
            },
            Athletes = new List<Athlete>
            {
                new() { Id = 10, FirstName = "Isaac", LastName = "Smith", Gender = Gender.Male, GraduationYear = 2027 },
                new() { Id = 11, FirstName = "Sam", LastName = "Runner", Gender = Gender.Male, GraduationYear = 2026 },
                new() { Id = 12, FirstName = "Tim", LastName = "Shores", Gender = Gender.Male, GraduationYear = 2025 },
                new() { Id = 13, FirstName = "Ben", LastName = "Fast", Gender = Gender.Male, GraduationYear = 2028 },
                new() { Id = 14, FirstName = "Cal", LastName = "Quick", Gender = Gender.Male, GraduationYear = 2026 },
                new() { Id = 15, FirstName = "Riley", LastName = "Evans", Gender = Gender.Female, GraduationYear = 2026 },
                // same name, an earlier era: the current student should win
                new() { Id = 16, FirstName = "Sam", LastName = "Runner", Gender = Gender.Male, GraduationYear = 2005 },
                new() { Id = 17, FirstName = "Jon", LastName = "Smythe", Gender = Gender.Male, GraduationYear = 2027 },
            },
        };
        var n = 1;
        foreach (var o in opponents)
            ctx.Participants.Add(new MeetParticipant { Id = n, MeetId = 7, SortOrder = n++, School = new School { Name = o } });
        return ctx;
    }

    private static ResultsFile File(params ResultsFileResult[] results) => new() { Results = results.ToList() };

    private static ResultsFileResult R(string gender, string ev, string athlete, string mark, int? place = null) =>
        new() { Gender = gender, Event = ev, Athlete = athlete, Mark = mark, Place = place };

    [Theory]
    [InlineData("100M", 1)]
    [InlineData("100", 1)]
    [InlineData("100 m", 1)]
    [InlineData("100 Meters", 1)]
    [InlineData("shot", 3)]
    [InlineData("Shot Put", 3)]
    [InlineData("4x100 Relay", 4)]
    [InlineData("4x100", 4)]
    [InlineData("LJ", 8)]
    public void Resolves_event_names_loosely(string name, int eventId)
    {
        var ev = MeetImportPlanner.ResolveEvent(Gender.Male, name, Context().Events);
        ev!.Id.Should().Be(eventId);
    }

    [Fact]
    public void Same_event_name_resolves_by_gender()
    {
        var ctx = Context();
        MeetImportPlanner.ResolveEvent(Gender.Female, "100M", ctx.Events)!.Id.Should().Be(2);
        MeetImportPlanner.ResolveEvent(Gender.Female, "Shot Put", ctx.Events).Should().BeNull("no girls shot put in this context");
    }

    [Theory]
    [InlineData("Boys", Gender.Male)]
    [InlineData("g", Gender.Female)]
    [InlineData("Mixed", Gender.Mixed)]
    public void Parses_gender(string text, Gender expected) => MeetImportPlanner.ParseGender(text).Should().Be(expected);

    [Fact]
    public void New_result_is_planned_with_parsed_time_and_athlete()
    {
        var plan = MeetImportPlanner.Plan(File(R("Boys", "100M", "Isaac Smith", "11.23", 1)), Context(MeetType.Dual, "Ravenna"));

        var row = plan.Rows.Single();
        row.State.Should().Be(ImportRowState.New);
        row.AthleteId.Should().Be(10);
        row.TimeSeconds.Should().BeApproximately(11.23, 0.0001);
        row.MarkText.Should().Be("11.23");
        row.Placings.Should().ContainSingle().Which.Should().Match<PlanPlacing>(p => p.MeetParticipantId == 1 && p.Place == 1);
    }

    [Fact]
    public void Field_marks_parse_to_inches_and_minutes_to_seconds()
    {
        var plan = MeetImportPlanner.Plan(File(R("Boys", "Shot Put", "Tim Shores", "38' 8\""), R("Boys", "800M", "Ben Fast", "2:05.40")), Context());

        plan.Rows[0].DistanceInches.Should().Be(464);
        plan.Rows[1].TimeSeconds.Should().BeApproximately(125.4, 0.0001);
    }

    [Fact]
    public void Status_results_have_no_mark()
    {
        var file = File(new ResultsFileResult { Gender = "Boys", Event = "Shot Put", Athlete = "Tim Shores", Status = "nm" });
        var row = MeetImportPlanner.Plan(file, Context()).Rows.Single();

        row.State.Should().Be(ImportRowState.New);
        row.Status.Should().Be(ResultStatus.NoMark);
        row.TimeSeconds.Should().BeNull();
        row.DistanceInches.Should().BeNull();
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("")]
    public void Unreadable_or_missing_marks_are_problems(string mark)
    {
        var row = MeetImportPlanner.Plan(File(R("Boys", "100M", "Isaac Smith", mark)), Context()).Rows.Single();
        row.State.Should().Be(ImportRowState.Problem);
        row.Problems.Should().NotBeEmpty();
    }

    [Fact]
    public void Unknown_event_is_a_problem()
    {
        var row = MeetImportPlanner.Plan(File(R("Boys", "Javelin", "Isaac Smith", "100'")), Context()).Rows.Single();
        row.State.Should().Be(ImportRowState.Problem);
        row.Problems.Single().Should().Contain("Javelin");
    }

    [Fact]
    public void Athlete_name_matches_ignoring_case_punctuation_and_last_first_order()
    {
        var plan = MeetImportPlanner.Plan(File(
            R("Boys", "100M", "isaac smith", "11.2"),
            R("Boys", "800M", "Smith, Isaac", "2:10.0")), Context());

        plan.Rows.Should().OnlyContain(r => r.AthleteId == 10);
    }

    [Fact]
    public void Duplicate_names_prefer_the_athlete_of_the_right_era()
    {
        var row = MeetImportPlanner.Plan(File(R("Boys", "100M", "Sam Runner", "11.5")), Context()).Rows.Single();
        row.AthleteId.Should().Be(11);
    }

    [Fact]
    public void Near_miss_name_is_a_problem_with_candidates_and_can_be_overridden()
    {
        var file = File(R("Boys", "100M", "Isaac Smyth", "11.5"));
        var first = MeetImportPlanner.Plan(file, Context()).Rows.Single();

        first.State.Should().Be(ImportRowState.Problem);
        first.Candidates.Select(c => c.Id).Should().Contain(10);

        var fixedRow = MeetImportPlanner.Plan(file, Context(), new Dictionary<int, int> { [0] = 10 }).Rows.Single();
        fixedRow.State.Should().Be(ImportRowState.New);
        fixedRow.AthleteId.Should().Be(10);
    }

    [Fact]
    public void Girls_event_does_not_match_a_boy()
    {
        var row = MeetImportPlanner.Plan(File(R("Girls", "100M", "Isaac Smith", "13.0")), Context()).Rows.Single();
        row.State.Should().Be(ImportRowState.Problem);
        row.Problems.Single().Should().Contain("No athlete");
    }

    [Fact]
    public void Relay_needs_the_right_number_of_known_athletes()
    {
        var ok = new ResultsFileResult { Gender = "Boys", Event = "4x100M Relay", Athletes = new() { "Isaac Smith", "Sam Runner", "Ben Fast", "Cal Quick" }, Mark = "46.1" };
        var shortTeam = new ResultsFileResult { Gender = "Boys", Event = "4x100M Relay", Athletes = new() { "Isaac Smith", "Sam Runner" }, Mark = "46.1" };

        var plan = MeetImportPlanner.Plan(File(ok, shortTeam), Context());

        plan.Rows[0].State.Should().Be(ImportRowState.New);
        plan.Rows[0].IsRelay.Should().BeTrue();
        plan.Rows[0].RelayAthleteIds.Should().BeEquivalentTo(new[] { 10, 11, 13, 14 });
        plan.Rows[1].State.Should().Be(ImportRowState.Problem);
    }

    [Fact]
    public void Dual_with_one_opponent_accepts_a_bare_place_but_two_opponents_need_names()
    {
        var one = MeetImportPlanner.Plan(File(R("Boys", "100M", "Isaac Smith", "11.2", place: 2)), Context(MeetType.Dual, "Ravenna")).Rows.Single();
        one.Placings.Single().OpponentLabel.Should().Be("vs. Ravenna");

        var two = MeetImportPlanner.Plan(File(R("Boys", "100M", "Isaac Smith", "11.2", place: 2)), Context(MeetType.DoubleDual, "Ravenna", "Streetsboro")).Rows.Single();
        two.State.Should().Be(ImportRowState.Problem);
    }

    [Fact]
    public void Double_dual_places_are_per_opponent_and_unknown_opponents_are_problems()
    {
        var good = R("Boys", "100M", "Isaac Smith", "11.2");
        good.Placings = new() { new() { Opponent = "Ravenna", Place = 1 }, new() { Opponent = "streetsboro", Place = 3 } };
        var bad = R("Boys", "800M", "Ben Fast", "2:10");
        bad.Placings = new() { new() { Opponent = "Akron", Place = 1 } };

        var plan = MeetImportPlanner.Plan(File(good, bad), Context(MeetType.DoubleDual, "Ravenna", "Streetsboro"));

        plan.Rows[0].Placings.Select(p => (p.MeetParticipantId, p.Place)).Should().Equal((1, 1), (2, 3));
        plan.Rows[1].State.Should().Be(ImportRowState.Problem);
    }

    [Fact]
    public void Invitational_place_is_overall()
    {
        var row = MeetImportPlanner.Plan(File(R("Boys", "100M", "Isaac Smith", "11.2", place: 4)), Context(MeetType.Invitational)).Rows.Single();
        var placing = row.Placings.Single();
        placing.MeetParticipantId.Should().BeNull();
        placing.Place.Should().Be(4);
    }

    [Fact]
    public void Stored_results_are_recognised_as_already_there_or_different()
    {
        var ctx = Context();
        ctx.Performances.Add(new ExistingPerformance { Id = 100, EventId = 1, AthleteId = 10, TimeSeconds = 11.23 });
        ctx.Performances.Add(new ExistingPerformance { Id = 101, EventId = 5, AthleteId = 13, TimeSeconds = 130.0 });

        var plan = MeetImportPlanner.Plan(File(
            R("Boys", "100M", "Isaac Smith", "11.23"),
            R("Boys", "800M", "Ben Fast", "2:05.4")), ctx);

        plan.Rows[0].State.Should().Be(ImportRowState.AlreadyThere);
        plan.Rows[0].ExistingPerformanceId.Should().Be(100);
        plan.Rows[1].State.Should().Be(ImportRowState.Differs);
        plan.Rows[1].ExistingMarkText.Should().Be("2:10.00");
        plan.Rows[1].DefaultInclude.Should().BeFalse("replacing a stored mark must be an explicit choice");
    }

    [Fact]
    public void Same_mark_with_a_missing_place_only_adds_the_place()
    {
        var ctx = Context(MeetType.Dual, "Ravenna");
        ctx.Performances.Add(new ExistingPerformance { Id = 100, EventId = 1, AthleteId = 10, TimeSeconds = 11.23 });

        var row = MeetImportPlanner.Plan(File(R("Boys", "100M", "Isaac Smith", "11.23", place: 1)), ctx).Rows.Single();

        row.State.Should().Be(ImportRowState.AddsPlacings);
        row.DefaultInclude.Should().BeTrue();
    }

    [Fact]
    public void A_different_stored_place_counts_as_a_difference()
    {
        var ctx = Context(MeetType.Dual, "Ravenna");
        ctx.Performances.Add(new ExistingPerformance { Id = 100, EventId = 1, AthleteId = 10, TimeSeconds = 11.23 });
        ctx.Placings.Add(new MeetPlacing { PerformanceId = 100, MeetParticipantId = 1, Place = 2 });

        MeetImportPlanner.Plan(File(R("Boys", "100M", "Isaac Smith", "11.23", place: 1)), ctx).Rows.Single().State.Should().Be(ImportRowState.Differs);
    }

    [Fact]
    public void The_same_athlete_and_event_twice_in_a_file_is_flagged()
    {
        var plan = MeetImportPlanner.Plan(File(R("Boys", "100M", "Isaac Smith", "11.2"), R("Boys", "100M", "Isaac Smith", "11.3")), Context());
        plan.Rows[0].State.Should().Be(ImportRowState.New);
        plan.Rows[1].State.Should().Be(ImportRowState.Problem);
    }

    [Fact]
    public void A_fifth_event_is_a_warning_not_a_block()
    {
        var plan = MeetImportPlanner.Plan(File(
            R("Boys", "100M", "Isaac Smith", "11.2"), R("Boys", "800M", "Isaac Smith", "2:10"),
            R("Boys", "1600M", "Isaac Smith", "5:00"), R("Boys", "3200M", "Isaac Smith", "11:00"),
            R("Boys", "Long Jump", "Isaac Smith", "18' 0\"")), Context());

        plan.Rows.Should().OnlyContain(r => r.State == ImportRowState.New);
        plan.Rows.Should().OnlyContain(r => r.Warnings.Count == 1);
    }

    [Fact]
    public void Entry_rows_are_new_unless_an_entry_or_result_exists()
    {
        var ctx = Context();
        ctx.Entries.Add(new MeetEntryDto { Id = 1, EventId = 1, AthleteId = 10 });
        var file = new ResultsFile
        {
            Entries = new()
            {
                new() { Gender = "Boys", Event = "100M", Athlete = "Isaac Smith" },
                new() { Gender = "Boys", Event = "100M", Athlete = "Ben Fast" },
            }
        };

        var plan = MeetImportPlanner.Plan(file, ctx);

        plan.Rows[0].State.Should().Be(ImportRowState.AlreadyThere);
        plan.Rows[1].State.Should().Be(ImportRowState.New);
        plan.Rows.Should().OnlyContain(r => r.Kind == ImportRowKind.Entry);
    }

    [Fact]
    public void A_file_for_another_meet_is_rejected()
    {
        var file = File(R("Boys", "100M", "Isaac Smith", "11.2"));
        file.Meet = new ResultsFileMeet { Id = 99, Name = "Somewhere else" };

        MeetImportPlanner.Plan(file, Context()).FileProblems.Should().ContainSingle().Which.Should().Contain("99");
    }

    [Fact]
    public void Parses_json_with_comments_trailing_commas_and_numeric_marks()
    {
        var file = ResultsFile.Parse("""
            {
              // from the scan
              "version": 1,
              "results": [
                { "gender": "Boys", "event": "100M", "athlete": "Isaac Smith", "mark": 11.23, "place": 1, },
              ]
            }
            """);

        file.Results.Single().Mark.Should().Be("11.23");
    }

    [Theory]
    [InlineData("")]
    [InlineData("{ not json")]
    public void Bad_files_raise_a_readable_error(string json) =>
        FluentActions.Invoking(() => ResultsFile.Parse(json)).Should().Throw<ResultsFileException>();
}
