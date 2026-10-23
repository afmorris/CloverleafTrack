using CloverleafTrack.DataAccess.Dtos;
using CloverleafTrack.Models;
using CloverleafTrack.Models.Enums;
using CloverleafTrack.Services;
using CloverleafTrack.ViewModels.Meets;
using CloverleafTrack.ViewModels.Scoring;
using CloverleafTrack.ViewModels.Seasons;
using CloverleafTrack.Web.Pdf;
using FluentAssertions;
using Environment = CloverleafTrack.Models.Enums.Environment;

namespace CloverleafTrack.Tests.Unit.Services;

public class SeasonWrapCalculatorTests
{
    private static readonly Season Season = new()
    {
        Id = 1, Name = "2025-2026", StartDate = new DateTime(2025, 12, 1), EndDate = new DateTime(2026, 6, 30), IsCurrentSeason = true, ScoringEnabled = true
    };

    private static int _id;

    private static SeasonWrapMarkDto Run(string first, string last, double seconds, string meet = "Meet A", int meetId = 1, string date = "2026-04-08",
        bool pr = false, double? prev = null, bool sr = false, int? rank = null, byte? pct = null, int grad = 2027, int eventId = 10, string ev = "100M",
        Gender gender = Gender.Male, Environment env = Environment.Outdoor) => new()
    {
        PerformanceId = ++_id, AthleteId = Math.Abs((first + last).GetHashCode()) % 100000, AthleteFirstName = first, AthleteLastName = last,
        GraduationYear = grad, EventId = eventId, EventName = ev, EventGender = gender, Environment = env, EventType = EventType.Running,
        TimeSeconds = seconds, PRAtTime = pr, PreviousBest = prev, SRAtTime = sr, AllTimeRank = rank, Percentile = pct,
        MeetId = meetId, MeetName = meet, MeetDate = DateTime.Parse(date), MeetType = MeetType.Dual
    };

    private static SeasonWrapMarkDto Throw(string first, string last, double inches, double? prev = null, bool pr = false, string date = "2026-04-08", int meetId = 1) =>
        new()
        {
            PerformanceId = ++_id, AthleteId = Math.Abs((first + last).GetHashCode()) % 100000, AthleteFirstName = first, AthleteLastName = last,
            GraduationYear = 2027, EventId = 20, EventName = "Shot Put", EventGender = Gender.Male, Environment = Environment.Outdoor, EventType = EventType.Field,
            DistanceInches = inches, PRAtTime = pr, PreviousBest = prev, MeetId = meetId, MeetName = "Meet A", MeetDate = DateTime.Parse(date)
        };

    private static SeasonWrapViewModel Build(List<SeasonWrapMarkDto> marks, List<SeasonWrapPriorBestDto>? priors = null,
        List<SeasonWrapTeamResultDto>? teams = null, Dictionary<int, MeetScoreViewModel>? scores = null, SeasonScoringViewModel? scoring = null) =>
        SeasonWrapCalculator.Build(Season, marks, priors ?? new(), teams ?? new(), scores ?? new(), scoring);

    [Fact]
    public void No_marks_gives_an_empty_wrap()
    {
        var vm = Build(new());
        vm.HasData.Should().BeFalse();
        vm.SchoolRecords.Should().BeEmpty();
    }

    [Fact]
    public void Seasons_before_the_complete_data_era_leave_out_pr_based_sections()
    {
        var old = new Season { Id = 2, Name = "2015-2016", StartDate = new DateTime(2015, 12, 1), EndDate = new DateTime(2016, 6, 30) };
        var marks = new List<SeasonWrapMarkDto>
        {
            Run("Ann", "Lee", 12.0, pr: true, prev: 12.5, pct: 80, date: "2016-04-01"),
            Run("Bo", "Kim", 12.4, pr: true, prev: 12.6, pct: 70, date: "2016-04-08", meetId: 2, meet: "Meet B"),
        };

        var vm = SeasonWrapCalculator.Build(old, marks, new List<SeasonWrapPriorBestDto>(), new List<SeasonWrapTeamResultDto>(), new Dictionary<int, MeetScoreViewModel>(), null);

        vm.IsPartialData.Should().BeTrue();
        vm.Breakouts.Should().BeEmpty();
        vm.Improvements.Should().BeEmpty();
        vm.Progression.Should().BeEmpty();
        vm.Standouts.Should().NotBeEmpty("percentile standouts still work on partial data");
        Build(marks).IsPartialData.Should().BeFalse("2025-26 is in the complete era");
    }

    [Fact]
    public void Headline_counts_use_the_at_the_time_flags_and_count_athletes_once()
    {
        var vm = Build(new()
        {
            Run("Ann", "Lee", 12.5, pr: true),                     // debut counts as a PR
            Run("Ann", "Lee", 12.1, pr: true, prev: 12.5),         // improvement, later beaten still counts
            Run("Ann", "Lee", 12.3),
            Run("Bo", "Kim", 11.9, meetId: 2, meet: "Meet B", date: "2026-04-15"),
        });

        vm.Meets.Should().Be(2);
        vm.Results.Should().Be(4);
        vm.AthletesCompeted.Should().Be(2);
        vm.PRs.Should().Be(2);
        vm.AthletesWithPRs.Should().Be(1);
        vm.ThroughDate.Should().Be(new DateTime(2026, 4, 15));
    }

    [Fact]
    public void School_records_are_one_row_per_event_with_the_best_mark_and_how_often_it_fell()
    {
        var vm = Build(new()
        {
            Run("Ann", "Lee", 12.5, sr: true, rank: null),
            Run("Cy", "Poe", 12.0, sr: true, rank: 1, date: "2026-04-15", meetId: 2),
            Run("Di", "Fox", 11.0, sr: false, date: "2026-04-22", meetId: 3),
        });

        var record = vm.SchoolRecords.Should().ContainSingle().Subject;
        vm.SchoolRecordsSet.Should().Be(1);
        vm.SchoolRecordBreaks.Should().Be(2);
        record.Mark.Should().Be("12.00");
        record.Holders.Single().Name.Should().Be("Cy Poe");
        record.TimesBroken.Should().Be(2);
        record.StillRecord.Should().BeTrue();
    }

    [Fact]
    public void Breakouts_ignore_debuts_and_rank_by_number_of_real_pr()
    {
        var vm = Build(new()
        {
            Run("Ann", "Lee", 13.0, pr: true),                       // debut: not a breakout PR
            Run("Ann", "Lee", 12.5, pr: true, prev: 13.0),
            Run("Bo", "Kim", 12.0, pr: true, prev: 12.4),
            Run("Bo", "Kim", 11.8, pr: true, prev: 12.0),
            Run("Bo", "Kim", 11.7, pr: true, prev: 11.8),
        });

        vm.Breakouts.Select(b => (b.Athlete.Name, b.PRs)).Should().Equal(("Bo Kim", 3), ("Ann Lee", 1));
        vm.Breakouts[0].Highlight.Should().Contain("100M").And.Contain("12.4").And.Contain("12");
    }

    [Fact]
    public void Improvements_compare_the_season_best_with_the_best_before_the_season()
    {
        var ann = Run("Ann", "Lee", 12.0);
        var shot = Throw("Gus", "Ray", 480);
        var vm = Build(new() { ann, Run("Ann", "Lee", 12.4), shot, Run("New", "Kid", 15.0) },
            priors: new()
            {
                new() { AthleteId = ann.AthleteId!.Value, EventId = 10, BestTime = 13.0 },
                new() { AthleteId = shot.AthleteId!.Value, EventId = 20, BestDistance = 420 },
            });

        vm.Improvements.Select(i => i.Athlete.Name).Should().Equal("Gus Ray", "Ann Lee");   // 14.3% then 7.7%; "New Kid" has no prior
        vm.Improvements[0].Change.Should().Be("5' 0\" farther");
        vm.Improvements[1].Change.Should().Be("1.00s faster");
        vm.Improvements[1].Before.Should().Be("13.00");
    }

    [Fact]
    public void Standouts_take_each_athletes_best_percentile_once()
    {
        var vm = Build(new()
        {
            Run("Ann", "Lee", 12.0, pct: 90),
            Run("Ann", "Lee", 25.0, pct: 95, ev: "200M", eventId: 11),
            Run("Bo", "Kim", 11.0, pct: 92),
            Run("No", "Pct", 10.0, pct: null),
        });

        vm.Standouts.Select(s => (s.Athlete.Name, s.Percentile)).Should().Equal(("Ann Lee", 95), ("Bo Kim", 92));
    }

    [Fact]
    public void Seniors_are_the_class_graduating_at_the_end_of_the_season()
    {
        var vm = Build(new()
        {
            Run("Sen", "Ior", 12.0, grad: 2026, pct: 80, pr: true, prev: 12.5),
            Run("Jun", "Ior", 12.0, grad: 2027, pct: 99),
        });

        vm.SeniorClassYear.Should().Be(2026);
        var senior = vm.Seniors.Should().ContainSingle().Subject;
        senior.Athlete.Name.Should().Be("Sen Ior");
        senior.PRs.Should().Be(1);
        senior.BestPercentile.Should().Be(80);
    }

    [Fact]
    public void Dual_record_counts_wins_losses_and_ties_per_gender()
    {
        TeamResult Row(int meet, string opp, Gender g, decimal us, decimal them) => new() { MeetId = meet, MeetName = $"Dual {meet}", MeetDate = new DateTime(2026, 4, meet), OpponentName = opp, Gender = g, OurScore = us, OpponentScore = them };
        var vm = Build(new() { Run("A", "B", 12) }, teams: new()
        {
            Row(1, "Ravenna", Gender.Male, 83, 54), Row(1, "Ravenna", Gender.Female, 112, 16),
            Row(1, "Streetsboro", Gender.Male, 52, 85), Row(1, "Streetsboro", Gender.Female, 70, 70),
            Row(2, "Woodridge", Gender.Male, 27.5m, 109.5m),
        });

        vm.DualRecord!.Boys.Text.Should().Be("1-2");
        vm.DualRecord.Girls.Text.Should().Be("1-0-1");
        vm.DualRecord.Duals.Select(d => (d.Opponent, d.Boys, d.Girls)).Should().Equal(
            ("Ravenna", "W 83-54", "W 112-16"), ("Streetsboro", "L 52-85", "T 70-70"), ("Woodridge", "L 27.5-109.5", ""));
    }

    [Fact]
    public void Best_meets_are_the_top_full_basis_meet_scores()
    {
        var marks = new List<SeasonWrapMarkDto>
        {
            Run("A", "A", 12, meetId: 1, meet: "One"), Run("B", "B", 12, meetId: 2, meet: "Two"), Run("C", "C", 12, meetId: 3, meet: "Three"),
            Run("D", "D", 12, meetId: 4, meet: "Four"), Run("E", "E", 12, meetId: 5, meet: "Five")
        };
        var scores = new Dictionary<int, MeetScoreViewModel>
        {
            [1] = new() { Score = 60, Basis = MeetScoreBasis.Full, PRs = 3 },
            [2] = new() { Score = 80, Basis = MeetScoreBasis.Full, PRs = 9 },
            [3] = new() { Score = 70, Basis = MeetScoreBasis.Full },
            [4] = new() { Score = 99, Basis = MeetScoreBasis.PeakOnly },      // not comparable: excluded
            [5] = new() { Score = 50, Basis = MeetScoreBasis.Full },
        };

        Build(marks, scores: scores).BestMeets.Select(m => m.Name).Should().Equal("Two", "Three", "One");
    }

    [Fact]
    public void Scoring_leaders_use_split_points_and_skip_zero_scorers()
    {
        AthleteScoreRowViewModel Row(string f, string l, decimal split) => new() { AthleteFirstName = f, AthleteLastName = l, AthleteSlug = $"{f}-{l}".ToLower(), TotalSplitPoints = split, TotalFullPoints = split + 100 };
        var scoring = new SeasonScoringViewModel
        {
            Boys = new() { Row("A", "One", 10), Row("B", "Two", 30), Row("C", "Zero", 0) },
            Girls = new() { Row("G", "One", 5) }
        };

        var vm = Build(new() { Run("A", "B", 12) }, scoring: scoring);

        vm.BoysLeaders.Select(l => l.Athlete.Name).Should().Equal("B Two", "A One");
        vm.GirlsLeaders.Should().ContainSingle();
    }

    [Fact]
    public void Progression_has_one_point_per_meet_in_date_order()
    {
        var vm = Build(new()
        {
            Run("A", "A", 12, meetId: 2, meet: "Later", date: "2026-05-01", pr: true, pct: 60),
            Run("B", "B", 12, meetId: 2, meet: "Later", date: "2026-05-01", pct: 40),
            Run("C", "C", 12, meetId: 1, meet: "Earlier", date: "2026-04-01", sr: true),
        });

        vm.Progression.Select(p => (p.Name, p.Results, p.PRs, p.SchoolRecords, p.AveragePercentile)).Should().Equal(
            ("Earlier", 1, 0, 1, 0d), ("Later", 2, 1, 0, 50d));
    }

    // ---- poster ----------------------------------------------------------------------------------------------

    [Fact]
    public void Poster_is_a_one_page_pdf_even_for_a_full_season()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        var marks = new List<SeasonWrapMarkDto>();
        for (var m = 1; m <= 25; m++)
            for (var a = 0; a < 6; a++)
                marks.Add(Run($"Ath{a}", $"M{m}x{a}", 12 + a * 0.1, meetId: m, meet: $"Meet {m}", date: new DateTime(2026, 3, 1).AddDays(m * 3).ToString("yyyy-MM-dd"),
                    pr: a % 2 == 0, prev: a % 2 == 0 ? 13 : null, sr: m % 5 == 0 && a == 0, pct: (byte)(40 + m + a), eventId: 100 + a, ev: $"Event {a}", rank: a == 0 ? 3 : null));
        var vm = Build(marks, teams: new() { new() { MeetId = 1, MeetName = "Dual", MeetDate = new DateTime(2026, 4, 1), OpponentName = "X", Gender = Gender.Male, OurScore = 80, OpponentScore = 50 } });

        var pdf = SeasonWrapPoster.Generate(vm, "https://example.com");

        System.Text.Encoding.ASCII.GetString(pdf, 0, 5).Should().Be("%PDF-");
        SeasonWrapPoster.PageCount(vm, "https://example.com").Should().Be(1);
    }

    private class TeamResult : SeasonWrapTeamResultDto { }
}
