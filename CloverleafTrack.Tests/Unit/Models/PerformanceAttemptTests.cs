using CloverleafTrack.Models;
using FluentAssertions;

namespace CloverleafTrack.Tests.Unit.Models;

public class PerformanceAttemptTests
{
    // -------------------------------------------------------------------------
    // PerformanceAttempt.IsValid
    // -------------------------------------------------------------------------

    [Fact]
    public void IsValid_TrueForAttemptWithDistanceAndNoFlags()
    {
        var attempt = new PerformanceAttempt { AttemptNumber = 1, DistanceInches = 300 };
        attempt.IsValid.Should().BeTrue();
    }

    [Fact]
    public void IsValid_FalseWhenFoul()
    {
        var attempt = new PerformanceAttempt { AttemptNumber = 1, IsFoul = true };
        attempt.IsValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_FalseWhenPass()
    {
        var attempt = new PerformanceAttempt { AttemptNumber = 1, IsPass = true };
        attempt.IsValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_FalseWhenDistanceIsNullAndNoFlags()
    {
        var attempt = new PerformanceAttempt { AttemptNumber = 1, DistanceInches = null };
        attempt.IsValid.Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // PerformanceAttemptSeries.BestValidDistance
    //
    // Acceptance-criteria scenarios (issue #12 attempt series):
    //   - all-foul series (no valid attempts)
    //   - single-attempt series
    //   - best attempt is not the last one taken (attempt order != best order)
    // -------------------------------------------------------------------------

    [Fact]
    public void BestValidDistance_ReturnsNull_ForAllFoulSeries()
    {
        // Decision: an all-foul/all-pass series has no new value to promote to
        // Performances.DistanceInches. The repository leaves the existing value
        // untouched in this case rather than nulling it out (which the
        // CK_Performances_DistanceOrTime constraint forbids for field events
        // anyway). This test locks in that BestValidDistance signals "no update"
        // via null, distinct from any real distance value including 0.
        var attempts = new List<PerformanceAttempt>
        {
            new() { AttemptNumber = 1, IsFoul = true },
            new() { AttemptNumber = 2, IsFoul = true },
            new() { AttemptNumber = 3, IsPass = true },
            new() { AttemptNumber = 4, IsFoul = true },
            new() { AttemptNumber = 5, IsPass = true },
            new() { AttemptNumber = 6, IsFoul = true },
        };

        PerformanceAttemptSeries.BestValidDistance(attempts).Should().BeNull();
    }

    [Fact]
    public void BestValidDistance_ReturnsNull_ForEmptySeries()
    {
        PerformanceAttemptSeries.BestValidDistance(new List<PerformanceAttempt>()).Should().BeNull();
    }

    [Fact]
    public void BestValidDistance_ReturnsTheOnlyValue_ForSingleAttemptSeries()
    {
        var attempts = new List<PerformanceAttempt>
        {
            new() { AttemptNumber = 1, DistanceInches = 456.25 }
        };

        PerformanceAttemptSeries.BestValidDistance(attempts).Should().Be(456.25);
    }

    [Fact]
    public void BestValidDistance_IgnoresFoulsAndPasses_InSingleAttemptSeries()
    {
        // "Single attempt" here means a series where only one attempt slot was
        // actually recorded with a valid mark, alongside foul/pass slots.
        var attempts = new List<PerformanceAttempt>
        {
            new() { AttemptNumber = 1, IsFoul = true },
            new() { AttemptNumber = 2, DistanceInches = 401.5 },
            new() { AttemptNumber = 3, IsPass = true },
        };

        PerformanceAttemptSeries.BestValidDistance(attempts).Should().Be(401.5);
    }

    [Fact]
    public void BestValidDistance_ReturnsMax_WhenBestAttemptIsNotTheLastOne()
    {
        // Attempt order != best order: the farthest mark (attempt 2) is neither
        // the first nor the last attempt taken.
        var attempts = new List<PerformanceAttempt>
        {
            new() { AttemptNumber = 1, DistanceInches = 300.0 },
            new() { AttemptNumber = 2, DistanceInches = 512.75 }, // best, middle of the series
            new() { AttemptNumber = 3, IsFoul = true },
            new() { AttemptNumber = 4, DistanceInches = 288.0 },
            new() { AttemptNumber = 5, IsPass = true },
            new() { AttemptNumber = 6, DistanceInches = 310.5 }, // last attempt, not the best
        };

        PerformanceAttemptSeries.BestValidDistance(attempts).Should().Be(512.75);
    }

    [Fact]
    public void BestValidDistance_IsOrderIndependent()
    {
        var inOrder = new List<PerformanceAttempt>
        {
            new() { AttemptNumber = 1, DistanceInches = 100 },
            new() { AttemptNumber = 2, DistanceInches = 200 },
            new() { AttemptNumber = 3, DistanceInches = 150 },
        };
        var shuffled = new List<PerformanceAttempt>
        {
            new() { AttemptNumber = 3, DistanceInches = 150 },
            new() { AttemptNumber = 1, DistanceInches = 100 },
            new() { AttemptNumber = 2, DistanceInches = 200 },
        };

        PerformanceAttemptSeries.BestValidDistance(inOrder)
            .Should().Be(PerformanceAttemptSeries.BestValidDistance(shuffled));
    }
}
