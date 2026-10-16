using CloverleafTrack.Models;
using CloverleafTrack.Models.Enums;
using CloverleafTrack.Services;
using FluentAssertions;

namespace CloverleafTrack.Tests.Unit.Services;

public class MeetScoreCalculatorTests
{
    private static readonly DateTime ModernSeason = new(2023, 12, 1);
    private static readonly DateTime LegacySeason = new(2010, 12, 1);

    private static int _perfId;

    private static MeetScoreMark Mark(
        int meetId, DateTime date, int? athleteId, int eventId = 1, double? value = 11.0, byte? pct = 90,
        bool field = false, int eventCount = 100, DateTime? season = null, MeetLevel level = MeetLevel.Varsity) => new()
    {
        PerformanceId = ++_perfId,
        MeetId = meetId,
        MeetDate = date,
        MeetLevel = level,
        SeasonStartDate = season ?? ModernSeason,
        EventId = eventId,
        AthleteId = athleteId,
        IsFieldEvent = field,
        Value = value,
        Percentile = pct,
        EventMarkCount = eventCount
    };

    /// <summary>
    /// Enough unrelated athlete-events (event 99, no history) to clear the minimum size for a Full score, so Improvement
    /// tests can exercise a meet without tripping the "too small to score" guard. Adds no eligible athlete-events.
    /// </summary>
    private static List<MeetScoreMark> Filler(int meetId, DateTime date) =>
        Enumerable.Range(0, MeetScoreCalculator.MinUnitsForFull)
            .Select(i => Mark(meetId, date, 9000 + meetId * 100 + i, eventId: 99, pct: 50)).ToList();

    [Fact]
    public void Peak_IsMeanOfTopFivePercentiles()
    {
        var marks = new[] { 100, 90, 80, 70, 60, 10, 5 }
            .Select((p, i) => Mark(1, new DateTime(2024, 4, 1), i + 1, pct: (byte)p)).ToList();

        MeetScoreCalculator.Peak(marks).Should().Be(80);
    }

    [Fact]
    public void PeakAndDepth_IgnoreEventsWithFewerThanTenMarks()
    {
        var marks = new List<MeetScoreMark>
        {
            Mark(1, new DateTime(2024, 4, 1), 1, pct: 100, eventCount: 9),
            Mark(1, new DateTime(2024, 4, 1), 2, pct: 50, eventCount: 10),
        };

        MeetScoreCalculator.Peak(marks).Should().Be(50);
        MeetScoreCalculator.Depth(marks).Should().Be(50);
    }

    [Fact]
    public void PeakAndDepth_IgnoreMarksWithoutPercentile()
    {
        var marks = new List<MeetScoreMark> { Mark(1, new DateTime(2024, 4, 1), 1, pct: null) };

        MeetScoreCalculator.Peak(marks).Should().BeNull();
        MeetScoreCalculator.Depth(marks).Should().BeNull();
    }

    [Fact]
    public void Depth_UsesBestPerAthleteEvent_AndRelayCountsOnce()
    {
        var d = new DateTime(2024, 4, 1);
        var marks = new List<MeetScoreMark>
        {
            Mark(1, d, 1, eventId: 1, pct: 90),
            Mark(1, d, 1, eventId: 1, pct: 70), // same athlete-event: only the best counts
            Mark(1, d, 1, eventId: 2, pct: 60), // same athlete, different event: counts separately
            Mark(1, d, null, eventId: 3, pct: 30), // relay
        };

        MeetScoreCalculator.Depth(marks).Should().BeApproximately((90 + 60 + 30) / 3.0, 1e-9);
    }

    [Fact]
    public void Depth_OnlyAveragesTheTop40()
    {
        var d = new DateTime(2024, 4, 1);
        var marks = Enumerable.Range(1, 40).Select(i => Mark(1, d, i, pct: 80))
            .Concat(Enumerable.Range(41, 100).Select(i => Mark(1, d, i, pct: 10))).ToList();

        MeetScoreCalculator.Depth(marks).Should().Be(80);
    }

    [Fact]
    public void Improvement_DetectsPrAgainstStrictlyEarlierMeets_RunningLowerIsBetter()
    {
        // Athlete 1 has two prior meets (11.0 then 10.9), then runs 10.8 (PR) at meet 3.
        var marks = new List<MeetScoreMark>
        {
            Mark(1, new DateTime(2024, 4, 1), 1, value: 11.0),
            Mark(2, new DateTime(2024, 4, 8), 1, value: 10.9),
            Mark(3, new DateTime(2024, 4, 15), 1, value: 10.8),
        };
        marks.AddRange(Filler(3, new DateTime(2024, 4, 15)));

        var result = MeetScoreCalculator.Calculate(marks)[3];

        result.PRs.Should().Be(1);
        result.EligibleAthleteEvents.Should().Be(1);
        // Rate for the 2-priors bucket is 1/1 = 100% from this data, so expected == actual -> par.
        result.Improvement.Should().BeApproximately(50 + 100 * (1 - result.ExpectedPRs) / 11, 1e-9);
    }

    [Fact]
    public void Improvement_FieldEvents_HigherIsBetter_AndEqualMarkIsNotAPr()
    {
        var marks = new List<MeetScoreMark>
        {
            Mark(1, new DateTime(2024, 4, 1), 1, value: 200, field: true),
            Mark(2, new DateTime(2024, 4, 8), 1, value: 200, field: true),
        };
        marks.AddRange(Filler(2, new DateTime(2024, 4, 8)));

        var result = MeetScoreCalculator.Calculate(marks)[2];

        result.PRs.Should().Be(0);
        result.EligibleAthleteEvents.Should().Be(1);
    }

    [Fact]
    public void Improvement_FirstMarkOfAthleteEvent_IsNotEligible()
    {
        var marks = new List<MeetScoreMark> { Mark(1, new DateTime(2024, 4, 1), 1) };
        marks.AddRange(Filler(1, new DateTime(2024, 4, 1)));

        var result = MeetScoreCalculator.Calculate(marks)[1];

        result.EligibleAthleteEvents.Should().Be(0);
        result.Improvement.Should().Be(50);
    }

    [Fact]
    public void Improvement_ExcludesRelays()
    {
        var marks = new List<MeetScoreMark>
        {
            Mark(1, new DateTime(2024, 4, 1), null, value: 45),
            Mark(2, new DateTime(2024, 4, 8), null, value: 44),
        };
        marks.AddRange(Filler(2, new DateTime(2024, 4, 8)));

        MeetScoreCalculator.Calculate(marks)[2].EligibleAthleteEvents.Should().Be(0);
    }

    [Fact]
    public void Improvement_ExpectedPrsUseEmpiricalRatePerPriorMeetCount_AndFewerPrsThanExpectedScoresBelowPar()
    {
        // Four athletes with one prior each. Three PR at meet 2 -> 75% empirical rate, so at meet 2 expected == actual.
        // At meet 3 each has two priors; none PR -> 0% rate, expected 0, also par. Use a mix to force a deficit instead:
        var marks = new List<MeetScoreMark>();
        for (var a = 1; a <= 4; a++)
        {
            marks.Add(Mark(1, new DateTime(2024, 4, 1), a, value: 11.0));
            marks.Add(Mark(2, new DateTime(2024, 4, 8), a, value: 10.9));
            // meet 3 (a different date): athletes 1,2 PR; 3,4 don't
            marks.Add(Mark(3, new DateTime(2024, 4, 15), a, value: a <= 2 ? 10.8 : 11.5));
        }
        marks.AddRange(Filler(2, new DateTime(2024, 4, 8)));
        marks.AddRange(Filler(3, new DateTime(2024, 4, 15)));

        var meet2 = MeetScoreCalculator.Calculate(marks)[2];
        meet2.PRs.Should().Be(4);
        meet2.ExpectedPRs.Should().BeApproximately(4, 1e-9); // bucket-1 rate is 100% in this data

        var meet3 = MeetScoreCalculator.Calculate(marks)[3];
        meet3.PRs.Should().Be(2);
        meet3.ExpectedPRs.Should().BeApproximately(2, 1e-9); // bucket-2 rate is 50% in this data
        meet3.Improvement.Should().Be(50);
    }

    [Fact]
    public void Improvement_PriorsBeforeCompleteEra_AreIgnored()
    {
        var marks = new List<MeetScoreMark>
        {
            Mark(1, new DateTime(2019, 4, 1), 1, value: 11.0, season: LegacySeason),
            Mark(2, new DateTime(2024, 4, 8), 1, value: 10.9),
        };
        marks.AddRange(Filler(2, new DateTime(2024, 4, 8)));

        MeetScoreCalculator.Calculate(marks)[2].EligibleAthleteEvents.Should().Be(0);
    }

    [Fact]
    public void Improvement_IsClampedTo0And100()
    {
        var marks = new List<MeetScoreMark>();
        // meet 1 has many early PRs vs. low expectation at meet 2? Build an extreme deficit: 1 prior each, rate dominated by others.
        for (var a = 1; a <= 30; a++)
        {
            marks.Add(Mark(1, new DateTime(2024, 4, 1), a, value: 11.0));
            marks.Add(Mark(2, new DateTime(2024, 4, 8), a, value: 10.9));
        }

        var r = MeetScoreCalculator.Calculate(marks)[2];
        r.Improvement.Should().BeInRange(0, 100);
    }

    /// <summary>One mark per distinct athlete (ids from 1000), all at the same percentile.</summary>
    private static List<MeetScoreMark> Athletes(
        int meetId, DateTime date, int count, byte pct = 80, DateTime? season = null, MeetLevel level = MeetLevel.Varsity, double value = 11.0) =>
        Enumerable.Range(0, count).Select(i => Mark(meetId, date, 1000 + i, value: value, pct: pct, season: season, level: level)).ToList();

    [Fact]
    public void FullMeet_ScoreIsWeightedBlendOfComponents()
    {
        var marks = Athletes(1, new DateTime(2024, 4, 1), MeetScoreCalculator.MinUnitsForFull, pct: 80);

        var r = MeetScoreCalculator.Calculate(marks)[1];

        r.Basis.Should().Be(MeetScoreBasis.Full);
        r.Peak.Should().Be(80);
        r.Depth.Should().Be(80);
        r.Improvement.Should().Be(50);
        r.Score.Should().BeApproximately(0.45 * 80 + 0.25 * 80 + 0.30 * 50, 1e-9);
    }

    [Fact]
    public void FullMeet_WithFewerThanMinimumAthleteEvents_IsNotScored()
    {
        // A state meet where only a handful of elite athletes qualified would otherwise average to a near-perfect score.
        var marks = Athletes(1, new DateTime(2024, 6, 1), MeetScoreCalculator.MinUnitsForFull - 1, pct: 99);

        MeetScoreCalculator.Calculate(marks).Should().NotContainKey(1);
    }

    [Fact]
    public void FullMeet_MinimumCountsAthleteEventsNotMarks()
    {
        // 8 athletes x 2 events = 16 athlete-events, but 14 distinct bests would not qualify: duplicates in one event collapse.
        var d = new DateTime(2024, 4, 1);
        var marks = Enumerable.Range(0, 14).Select(i => Mark(1, d, 1000 + i, eventId: 1, pct: 90)).ToList();
        marks.AddRange(Enumerable.Range(0, 14).Select(i => Mark(1, d, 1000 + i, eventId: 1, pct: 85))); // same athlete-event again

        MeetScoreCalculator.Calculate(marks).Should().NotContainKey(1);
    }

    [Fact]
    public void PreEraMeet_IsScoredOnPeakOnly()
    {
        var marks = new byte[] { 100, 90, 80, 70, 60 }
            .Select((p, i) => Mark(1, new DateTime(2010, 4, 1), i + 1, pct: p, season: LegacySeason)).ToList();

        var r = MeetScoreCalculator.Calculate(marks)[1];

        r.Basis.Should().Be(MeetScoreBasis.PeakOnly);
        r.Score.Should().Be(80);
        r.Depth.Should().BeNull();
        r.Improvement.Should().BeNull();
    }

    [Fact]
    public void PreEraMeet_WithFewerThanFiveScorableMarks_IsNotScored()
    {
        var marks = new byte[] { 100, 90, 80, 70 }
            .Select((p, i) => Mark(1, new DateTime(2010, 4, 1), i + 1, pct: p, season: LegacySeason)).ToList();

        MeetScoreCalculator.Calculate(marks).Should().NotContainKey(1);
    }

    [Fact]
    public void JvMeet_IsScoredOnImprovementOnly()
    {
        var n = MeetScoreCalculator.MinEligibleForImprovementOnly;
        var marks = Athletes(1, new DateTime(2024, 4, 1), n, pct: 5, value: 11.0);
        marks.AddRange(Athletes(2, new DateTime(2024, 4, 8), n, pct: 5, level: MeetLevel.JV, value: 10.9));

        var r = MeetScoreCalculator.Calculate(marks)[2];

        r.Basis.Should().Be(MeetScoreBasis.ImprovementOnly);
        r.Score.Should().Be(r.Improvement!.Value);
        r.EligibleAthleteEvents.Should().Be(n);
        r.Peak.Should().BeNull();
        r.Depth.Should().BeNull();
    }

    [Fact]
    public void JvMeet_WithTooFewEligibleAthleteEvents_IsNotScored()
    {
        var n = MeetScoreCalculator.MinEligibleForImprovementOnly - 1;
        var marks = Athletes(1, new DateTime(2024, 4, 1), n, pct: 5, value: 11.0);
        marks.AddRange(Athletes(2, new DateTime(2024, 4, 8), n, pct: 5, level: MeetLevel.JV, value: 10.9));

        MeetScoreCalculator.Calculate(marks).Should().NotContainKey(2);
    }

    [Fact]
    public void EraBoundary_IsSeasonStartDate_2020_12_01()
    {
        MeetScoreCalculator.IsCompleteEra(new DateTime(2020, 12, 1)).Should().BeTrue();
        MeetScoreCalculator.IsCompleteEra(new DateTime(2019, 12, 1)).Should().BeFalse();
    }
}
