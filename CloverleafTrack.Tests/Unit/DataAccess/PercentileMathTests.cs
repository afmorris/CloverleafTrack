using CloverleafTrack.Tests.TestSupport;
using FluentAssertions;
using static CloverleafTrack.Tests.TestSupport.PercentileMath;

namespace CloverleafTrack.Tests.Unit.DataAccess;

/// <summary>
/// Documents and exercises the percentile / median / Q1 / Q3 algorithm defined for
/// GitHub issue #4 ("Percentile foundation"). These tests run against
/// <see cref="PercentileMath"/> — a pure C# mirror of the T-SQL in
/// sp_RebuildLeaderboards (docs/schema.sql, Steps 11-12) — because no SQL Server
/// instance is available in this environment. See BRAIN.md for the note that the
/// production computation lives exclusively in the stored procedure; this mirror is
/// test-only scaffolding, not a second runtime pipeline.
/// </summary>
public class PercentileMathTests
{
    // -------------------------------------------------------------------------
    // Single-mark event
    // -------------------------------------------------------------------------

    [Fact]
    public void ComputePercentiles_SingleRunningMark_Returns100()
    {
        var marks = new[] { new Mark(PerformanceId: 1, Value: 11.24) };

        var result = ComputePercentiles(marks, betterIsHigher: false);

        result[1].Should().Be(100);
    }

    [Fact]
    public void ComputePercentiles_SingleFieldMark_Returns100()
    {
        var marks = new[] { new Mark(PerformanceId: 1, Value: 240.5) };

        var result = ComputePercentiles(marks, betterIsHigher: true);

        result[1].Should().Be(100);
    }

    // -------------------------------------------------------------------------
    // Ties
    // -------------------------------------------------------------------------

    [Fact]
    public void ComputePercentiles_TiedRunningMarks_GetIdenticalPercentile()
    {
        // 4 marks; the two middle marks (index 1 and 2) are tied at 11.05.
        var marks = new[]
        {
            new Mark(1, 10.90),
            new Mark(2, 11.05),
            new Mark(3, 11.05), // tied with #2
            new Mark(4, 11.80),
        };

        var result = ComputePercentiles(marks, betterIsHigher: false);

        result[2].Should().Be(result[3], "tied raw values must produce identical percentiles");
    }

    [Fact]
    public void ComputePercentiles_TiedFieldMarks_GetIdenticalPercentile()
    {
        var marks = new[]
        {
            new Mark(1, 250.0),
            new Mark(2, 240.0),
            new Mark(3, 240.0), // tied with #2
            new Mark(4, 200.0),
        };

        var result = ComputePercentiles(marks, betterIsHigher: true);

        result[2].Should().Be(result[3], "tied raw values must produce identical percentiles");
    }

    [Fact]
    public void ComputePercentiles_AllMarksTied_AllGetSamePercentile()
    {
        var marks = new[]
        {
            new Mark(1, 11.05),
            new Mark(2, 11.05),
            new Mark(3, 11.05),
        };

        var result = ComputePercentiles(marks, betterIsHigher: false);

        result.Values.Distinct().Should().ContainSingle();
    }

    // -------------------------------------------------------------------------
    // Field vs running direction
    // -------------------------------------------------------------------------

    [Fact]
    public void ComputePercentiles_RunningEvent_LowerTimeIsBetter_HighestPercentile()
    {
        var marks = new[]
        {
            new Mark(1, 10.90), // fastest = best
            new Mark(2, 11.50),
            new Mark(3, 12.10), // slowest = worst
        };

        var result = ComputePercentiles(marks, betterIsHigher: false);

        result[1].Should().BeGreaterThan(result[2]);
        result[2].Should().BeGreaterThan(result[3]);
    }

    [Fact]
    public void ComputePercentiles_FieldEvent_HigherDistanceIsBetter_HighestPercentile()
    {
        var marks = new[]
        {
            new Mark(1, 250.0), // farthest = best
            new Mark(2, 220.0),
            new Mark(3, 190.0), // shortest = worst
        };

        var result = ComputePercentiles(marks, betterIsHigher: true);

        result[1].Should().BeGreaterThan(result[2]);
        result[2].Should().BeGreaterThan(result[3]);
    }

    [Fact]
    public void ComputePercentiles_SameRawValues_FieldAndRunningDirectionsProduceOppositeOrdering()
    {
        // Identical raw values (e.g. "10, 20, 30") interpreted once as running times
        // (lower better) and once as field distances (higher better) must rank the
        // SAME performance ids in opposite order.
        var marks = new[] { new Mark(1, 10.0), new Mark(2, 20.0), new Mark(3, 30.0) };

        var runningResult = ComputePercentiles(marks, betterIsHigher: false); // 10.0 is best (lowest time)
        var fieldResult = ComputePercentiles(marks, betterIsHigher: true);    // 30.0 is best (highest distance)

        runningResult[1].Should().Be(fieldResult[3]);
        runningResult[3].Should().Be(fieldResult[1]);
    }

    // -------------------------------------------------------------------------
    // Relay population isolation
    // -------------------------------------------------------------------------

    [Fact]
    public void ComputePercentiles_DifferentEventPopulations_ComputeIndependently()
    {
        // Individual 100m Dash (EventId=1) and 4x100m Relay (EventId=2) are always
        // distinct Events rows (distinct EventKey), so in production they are
        // partitioned by EventId and never share a population. This test simulates
        // that: the same PerformanceId numbering space is reused across two
        // independently-computed populations, and neither leaks into the other's
        // percentile math — a fast time in the small relay population does not
        // change the percentile of an equally-fast time in the larger individual
        // population, because each is scoped to its own EventId's marks only.
        var individualEventMarks = new[]
        {
            new Mark(101, 10.90),
            new Mark(102, 11.20),
            new Mark(103, 11.50),
            new Mark(104, 11.80),
        };
        var relayEventMarks = new[]
        {
            new Mark(201, 42.10),
            new Mark(202, 43.00),
        };

        var individualResult = ComputePercentiles(individualEventMarks, betterIsHigher: false);
        var relayResult = ComputePercentiles(relayEventMarks, betterIsHigher: false);

        // Individual population (4 marks): best gets (4-1)/4*100 = 75.
        individualResult[101].Should().Be(75);
        // Relay population (2 marks), computed completely independently: best gets
        // (2-1)/2*100 = 50 — NOT influenced by the individual event's 4-mark population.
        relayResult[201].Should().Be(50);
    }

    [Fact]
    public void ComputePercentiles_TreatsAllMarksInThePopulationUniformly_NoAthleteIdSpecialCasing()
    {
        // The algorithm only ever sees (PerformanceId, Value) pairs — it has no concept
        // of AthleteId, so relay performances (AthleteId IS NULL in the DB) and
        // individual performances participate in percentile math identically once they
        // share an EventId. This documents that there is no special-casing to bypass.
        var mixedPopulation = new[]
        {
            new Mark(1, 42.10), // "relay" mark, hypothetically
            new Mark(2, 42.10), // "individual" mark, hypothetically — same event, tied value
            new Mark(3, 45.00),
        };

        var result = ComputePercentiles(mixedPopulation, betterIsHigher: false);

        result[1].Should().Be(result[2], "population membership is determined only by EventId, never by AthleteId");
    }

    // -------------------------------------------------------------------------
    // Median / Q1 / Q3
    // -------------------------------------------------------------------------

    [Fact]
    public void ComputeQuartiles_EvenCountPopulation_MedianIsAverageOfTwoMiddleValues()
    {
        // 10 sorted values (even count) — median must be the average of the 5th and
        // 6th order statistics (indices 4 and 5, zero-based).
        double[] values = [10.0, 10.90, 11.05, 11.05, 11.20, 11.35, 11.50, 11.50, 11.80, 12.00];

        var (median, _, _) = ComputeQuartiles(values, minimumSampleSize: 10);

        var expectedMedian = (values[4] + values[5]) / 2.0; // (11.20 + 11.35) / 2 = 11.275
        median.Should().Be(expectedMedian);
    }

    [Fact]
    public void ComputeQuartiles_FewerThanMinimumSampleSize_ReturnsAllNull()
    {
        // 8 marks — below the default 10-mark threshold — must yield NULL for all three.
        double[] values = [10.90, 11.05, 11.05, 11.20, 11.35, 11.50, 11.50, 11.80];

        var (median, q1, q3) = ComputeQuartiles(values);

        median.Should().BeNull();
        q1.Should().BeNull();
        q3.Should().BeNull();
    }

    [Fact]
    public void ComputeQuartiles_ExactlyAtMinimumSampleSize_ComputesValues()
    {
        double[] values = [10.0, 10.90, 11.05, 11.05, 11.20, 11.35, 11.50, 11.50, 11.80, 12.00];

        var (median, q1, q3) = ComputeQuartiles(values, minimumSampleSize: 10);

        median.Should().NotBeNull();
        q1.Should().NotBeNull();
        q3.Should().NotBeNull();
        q1!.Value.Should().BeLessThan(median!.Value);
        median.Value.Should().BeLessThan(q3!.Value);
    }

    [Fact]
    public void ComputeQuartiles_OddCountPopulation_MedianIsMiddleValue()
    {
        double[] values = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11]; // 11 values, middle = 6

        var (median, _, _) = ComputeQuartiles(values, minimumSampleSize: 10);

        median.Should().Be(6);
    }
}
