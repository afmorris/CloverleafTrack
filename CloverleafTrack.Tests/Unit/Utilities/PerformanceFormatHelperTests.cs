using CloverleafTrack.Web.Utilities;
using FluentAssertions;

namespace CloverleafTrack.Tests.Unit.Utilities;

public class PerformanceFormatHelperTests
{
    // -------------------------------------------------------------------------
    // ParseTime
    // -------------------------------------------------------------------------

    [Fact]
    public void ParseTime_ReturnsNull_ForNullInput()
    {
        PerformanceFormatHelper.ParseTime(null).Should().BeNull();
    }

    [Fact]
    public void ParseTime_ReturnsNull_ForEmptyString()
    {
        PerformanceFormatHelper.ParseTime("").Should().BeNull();
    }

    [Fact]
    public void ParseTime_ReturnsNull_ForWhitespace()
    {
        PerformanceFormatHelper.ParseTime("   ").Should().BeNull();
    }

    [Fact]
    public void ParseTime_ParsesSimpleDecimal()
    {
        PerformanceFormatHelper.ParseTime("11.24").Should().Be(11.24);
    }

    [Fact]
    public void ParseTime_ParsesSecondsWithSuffix()
    {
        PerformanceFormatHelper.ParseTime("11.24s").Should().Be(11.24);
    }

    [Fact]
    public void ParseTime_ParsesMinutesColonSeconds()
    {
        // 1 minute 23.45 seconds = 83.45 seconds
        PerformanceFormatHelper.ParseTime("1:23.45").Should().BeApproximately(83.45, 0.001);
    }

    [Fact]
    public void ParseTime_ParsesMinuteSuffixFormat()
    {
        PerformanceFormatHelper.ParseTime("1m23.45s").Should().BeApproximately(83.45, 0.001);
    }

    [Fact]
    public void ParseTime_HandlesExactlyOneMinute()
    {
        PerformanceFormatHelper.ParseTime("1:00.00").Should().Be(60.0);
    }

    [Fact]
    public void ParseTime_ReturnsNull_ForNonNumericInput()
    {
        PerformanceFormatHelper.ParseTime("not a time").Should().BeNull();
    }

    // -------------------------------------------------------------------------
    // FormatTime
    // -------------------------------------------------------------------------

    [Fact]
    public void FormatTime_FormatsSubMinuteTime()
    {
        PerformanceFormatHelper.FormatTime(11.24).Should().Be("11.24");
    }

    [Fact]
    public void FormatTime_FormatsExactlyOneMinute()
    {
        PerformanceFormatHelper.FormatTime(60.0).Should().Be("1:00.00");
    }

    [Fact]
    public void FormatTime_FormatsMinutesAndSeconds()
    {
        PerformanceFormatHelper.FormatTime(83.45).Should().Be("1:23.45");
    }

    [Fact]
    public void FormatTime_PadsSecondsWithLeadingZero()
    {
        // 65.5 seconds = 1 minute 5.5 seconds → "1:05.50"
        PerformanceFormatHelper.FormatTime(65.5).Should().Be("1:05.50");
    }

    // -------------------------------------------------------------------------
    // ParseDistance
    // -------------------------------------------------------------------------

    [Fact]
    public void ParseDistance_ReturnsNull_ForNullInput()
    {
        PerformanceFormatHelper.ParseDistance(null).Should().BeNull();
    }

    [Fact]
    public void ParseDistance_ReturnsNull_ForEmptyString()
    {
        PerformanceFormatHelper.ParseDistance("").Should().BeNull();
    }

    [Fact]
    public void ParseDistance_ParsesFeetAndInches()
    {
        // 19'4 = (19 * 12) + 4 = 232 inches
        PerformanceFormatHelper.ParseDistance("19'4").Should().Be(232.0);
    }

    [Fact]
    public void ParseDistance_ParsesFeetAndDecimalInches()
    {
        PerformanceFormatHelper.ParseDistance("19'4.5").Should().Be(232.5);
    }

    [Fact]
    public void ParseDistance_ParsesDashFormat()
    {
        PerformanceFormatHelper.ParseDistance("19-04").Should().Be(232.0);
    }

    [Fact]
    public void ParseDistance_ParsesTotalInches()
    {
        PerformanceFormatHelper.ParseDistance("234.5").Should().Be(234.5);
    }

    [Fact]
    public void ParseDistance_ParsesNaturalLanguage()
    {
        PerformanceFormatHelper.ParseDistance("19 feet 4 inches").Should().Be(232.0);
    }

    [Fact]
    public void ParseDistance_ReturnsNull_ForNonNumericInput()
    {
        PerformanceFormatHelper.ParseDistance("not a distance").Should().BeNull();
    }

    // -------------------------------------------------------------------------
    // FormatDistance
    // -------------------------------------------------------------------------

    [Fact]
    public void FormatDistance_FormatsWholeInches()
    {
        // 232 inches = 19 feet 4 inches
        PerformanceFormatHelper.FormatDistance(232.0).Should().Be("19' 4\"");
    }

    [Fact]
    public void FormatDistance_FormatsDecimalInches()
    {
        PerformanceFormatHelper.FormatDistance(232.5).Should().Be("19' 4.5\"");
    }

    [Fact]
    public void FormatDistance_FormatsExactFeet()
    {
        // 144 inches = exactly 12 feet
        PerformanceFormatHelper.FormatDistance(144.0).Should().Be("12' 0\"");
    }

    // -------------------------------------------------------------------------
    // FormatPerformance
    // -------------------------------------------------------------------------

    [Fact]
    public void FormatPerformance_FormatsTime_WhenTimeProvided()
    {
        PerformanceFormatHelper.FormatPerformance(11.24, null).Should().Be("11.24");
    }

    [Fact]
    public void FormatPerformance_FormatsDistance_WhenDistanceProvided()
    {
        PerformanceFormatHelper.FormatPerformance(null, 232.0).Should().Be("19' 4\"");
    }

    [Fact]
    public void FormatPerformance_ReturnsNA_WhenBothNull()
    {
        PerformanceFormatHelper.FormatPerformance(null, null).Should().Be("N/A");
    }

    // -------------------------------------------------------------------------
    // FormatImprovement
    // -------------------------------------------------------------------------

    [Fact]
    public void FormatImprovement_ShowsFaster_WhenTimeImproved()
    {
        // previous 12.0, current 11.5 → diff = 0.5 faster
        PerformanceFormatHelper.FormatImprovement(11.5, 12.0, null, null)
            .Should().Be("-0.50s faster");
    }

    [Fact]
    public void FormatImprovement_ShowsSlower_WhenTimeWorsened()
    {
        // previous 11.5, current 12.0 → diff = 0.5 slower
        PerformanceFormatHelper.FormatImprovement(12.0, 11.5, null, null)
            .Should().Be("+0.50s slower");
    }

    [Fact]
    public void FormatImprovement_ShowsFarther_WhenDistanceImproved()
    {
        // previous 228 inches (19'0"), current 240 inches (20'0") → +12 inches = +1'0" farther
        PerformanceFormatHelper.FormatImprovement(null, null, 240.0, 228.0)
            .Should().Be("+1' 0\" farther");
    }

    [Fact]
    public void FormatImprovement_ShowsShorter_WhenDistanceWorsened()
    {
        // previous 240 inches, current 228 inches → -12 inches = -1'0" shorter
        PerformanceFormatHelper.FormatImprovement(null, null, 228.0, 240.0)
            .Should().Be("-1' 0\" shorter");
    }

    [Fact]
    public void FormatImprovement_ReturnsEmpty_WhenAllNull()
    {
        PerformanceFormatHelper.FormatImprovement(null, null, null, null)
            .Should().BeEmpty();
    }

    // ParseTime / FormatTime round-trip
    [Theory]
    [InlineData("11.24", 11.24)]
    [InlineData("1:23.45", 83.45)]
    [InlineData("4:58.75", 298.75)]
    public void ParseTime_RoundTrip_WithFormatTime(string input, double expectedSeconds)
    {
        var parsed = PerformanceFormatHelper.ParseTime(input);
        parsed.Should().BeApproximately(expectedSeconds, 0.001);
        PerformanceFormatHelper.FormatTime(parsed!.Value).Should().Be(input);
    }

    // ParseDistance / FormatDistance round-trip
    // Input uses compact format (no space), FormatDistance always outputs "F' I\"" (with space)
    [Theory]
    [InlineData("19'4", 232.0, "19' 4\"")]
    [InlineData("42'6", 510.0, "42' 6\"")]
    [InlineData("12'0", 144.0, "12' 0\"")]
    public void ParseDistance_RoundTrip_WithFormatDistance(string input, double expectedInches, string expectedFormatted)
    {
        var parsed = PerformanceFormatHelper.ParseDistance(input);
        parsed.Should().Be(expectedInches);
        PerformanceFormatHelper.FormatDistance(parsed!.Value).Should().Be(expectedFormatted);
    }
}
