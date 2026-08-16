using CloverleafTrack.Web.Utilities;
using FluentAssertions;

namespace CloverleafTrack.Tests.Unit.Utilities;

public class PercentileHelperTests
{
    // -------------------------------------------------------------------------
    // Bucket boundaries (colors must match issue #19 / #21's shared table exactly)
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(1, "#8fc4f5")]
    [InlineData(14, "#8fc4f5")]
    [InlineData(15, "#6fa9e8")]
    [InlineData(34, "#6fa9e8")]
    [InlineData(35, "#5f96d4")]
    [InlineData(47, "#5f96d4")]
    [InlineData(48, "#a2aabb")]
    [InlineData(50, "#a2aabb")]
    [InlineData(52, "#a2aabb")]
    [InlineData(53, "#d98d84")]
    [InlineData(65, "#d98d84")]
    [InlineData(66, "#e8897d")]
    [InlineData(84, "#e8897d")]
    [InlineData(85, "#f09a8b")]
    [InlineData(100, "#f09a8b")]
    public void GetTextColor_ReturnsCorrectBucket(int percentile, string expectedHex)
    {
        PercentileHelper.GetTextColor(percentile).Should().Be(expectedHex);
    }

    [Fact]
    public void GetTextColor_ClampsBelowRange()
    {
        PercentileHelper.GetTextColor(0).Should().Be(PercentileHelper.GetTextColor(1));
    }

    [Fact]
    public void GetTextColor_ClampsAboveRange()
    {
        PercentileHelper.GetTextColor(101).Should().Be(PercentileHelper.GetTextColor(100));
    }

    [Theory]
    [InlineData(1, "#5aa3ea")]
    [InlineData(100, "#ef8272")]
    public void GetFillColor_ReturnsCorrectBucket(int percentile, string expectedHex)
    {
        PercentileHelper.GetFillColor(percentile).Should().Be(expectedHex);
    }

    [Theory]
    [InlineData(1, "#d8eafc")]
    [InlineData(100, "#fde3dc")]
    public void GetInkColor_ReturnsCorrectBucket(int percentile, string expectedHex)
    {
        PercentileHelper.GetInkColor(percentile).Should().Be(expectedHex);
    }

    // -------------------------------------------------------------------------
    // OrdinalSuffix
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(1, "st")]
    [InlineData(2, "nd")]
    [InlineData(3, "rd")]
    [InlineData(4, "th")]
    [InlineData(11, "th")]
    [InlineData(12, "th")]
    [InlineData(13, "th")]
    [InlineData(21, "st")]
    [InlineData(22, "nd")]
    [InlineData(23, "rd")]
    [InlineData(97, "th")]
    [InlineData(100, "th")]
    public void OrdinalSuffix_ReturnsCorrectSuffix(int n, string expected)
    {
        PercentileHelper.OrdinalSuffix(n).Should().Be(expected);
    }

    // -------------------------------------------------------------------------
    // GetBucketLabel / GetReading
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(1, "far below median")]
    [InlineData(20, "below median")]
    [InlineData(40, "slightly below median")]
    [InlineData(50, "at median")]
    [InlineData(60, "slightly above median")]
    [InlineData(75, "above median")]
    [InlineData(97, "far above median")]
    public void GetBucketLabel_ReturnsCorrectLabel(int percentile, string expectedLabel)
    {
        PercentileHelper.GetBucketLabel(percentile).Should().Be(expectedLabel);
    }

    [Fact]
    public void GetReading_MatchesIssueExample()
    {
        PercentileHelper.GetReading(97).Should().Be("97th percentile of program history — far above median");
    }

    [Fact]
    public void GetReading_UsesCorrectOrdinalForFirstPercentile()
    {
        PercentileHelper.GetReading(1).Should().Be("1st percentile of program history — far below median");
    }
}
