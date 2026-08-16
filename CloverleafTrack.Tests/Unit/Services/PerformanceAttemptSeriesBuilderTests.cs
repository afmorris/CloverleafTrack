using CloverleafTrack.Models;
using CloverleafTrack.Services;
using FluentAssertions;

namespace CloverleafTrack.Tests.Unit.Services;

public class PerformanceAttemptSeriesBuilderTests
{
    [Fact]
    public void BuildLookup_ReturnsEmptyDictionary_ForNoAttempts()
    {
        var lookup = PerformanceAttemptSeriesBuilder.BuildLookup(new List<PerformanceAttempt>());
        lookup.Should().BeEmpty();
    }

    [Fact]
    public void BuildLookup_GroupsAttemptsByPerformanceId()
    {
        var attempts = new List<PerformanceAttempt>
        {
            new() { PerformanceId = 1, AttemptNumber = 1, DistanceInches = 300 },
            new() { PerformanceId = 1, AttemptNumber = 2, DistanceInches = 320 },
            new() { PerformanceId = 2, AttemptNumber = 1, DistanceInches = 500 },
        };

        var lookup = PerformanceAttemptSeriesBuilder.BuildLookup(attempts);

        lookup.Should().ContainKey(1);
        lookup.Should().ContainKey(2);
        lookup[1].Attempts.Should().HaveCount(2);
        lookup[2].Attempts.Should().HaveCount(1);
    }

    [Fact]
    public void BuildLookup_OrdersAttemptsByAttemptNumber()
    {
        var attempts = new List<PerformanceAttempt>
        {
            new() { PerformanceId = 1, AttemptNumber = 3, DistanceInches = 300 },
            new() { PerformanceId = 1, AttemptNumber = 1, DistanceInches = 320 },
            new() { PerformanceId = 1, AttemptNumber = 2, IsFoul = true },
        };

        var lookup = PerformanceAttemptSeriesBuilder.BuildLookup(attempts);

        lookup[1].Attempts.Select(a => a.AttemptNumber).Should().ContainInOrder(1, 2, 3);
    }

    [Fact]
    public void BuildLookup_MarksBestAttempt_EvenWhenNotLastInOrder()
    {
        var attempts = new List<PerformanceAttempt>
        {
            new() { PerformanceId = 1, AttemptNumber = 1, DistanceInches = 300 },
            new() { PerformanceId = 1, AttemptNumber = 2, DistanceInches = 512.75 }, // best, middle
            new() { PerformanceId = 1, AttemptNumber = 3, DistanceInches = 310.5 },  // last, not best
        };

        var lookup = PerformanceAttemptSeriesBuilder.BuildLookup(attempts);

        var best = lookup[1].Attempts.Single(a => a.IsBest);
        best.AttemptNumber.Should().Be(2);
        best.DistanceInches.Should().Be(512.75);
    }

    [Fact]
    public void BuildLookup_NoAttemptMarkedBest_WhenSeriesIsAllFoul()
    {
        var attempts = new List<PerformanceAttempt>
        {
            new() { PerformanceId = 1, AttemptNumber = 1, IsFoul = true },
            new() { PerformanceId = 1, AttemptNumber = 2, IsPass = true },
        };

        var lookup = PerformanceAttemptSeriesBuilder.BuildLookup(attempts);

        lookup[1].Attempts.Should().NotContain(a => a.IsBest);
        lookup[1].HasAttempts.Should().BeTrue();
        lookup[1].ValidAttemptCount.Should().Be(0);
    }
}
