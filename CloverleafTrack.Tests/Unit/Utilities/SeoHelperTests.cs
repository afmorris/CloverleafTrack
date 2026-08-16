using System.Text.Json;
using CloverleafTrack.ViewModels.Shared;
using CloverleafTrack.Web.Utilities;
using FluentAssertions;

namespace CloverleafTrack.Tests.Unit.Utilities;

public class SeoHelperTests
{
    // -------------------------------------------------------------------------
    // Truncate
    // -------------------------------------------------------------------------

    [Fact]
    public void Truncate_ReturnsEmpty_ForNull()
    {
        SeoHelper.Truncate(null).Should().Be(string.Empty);
    }

    [Fact]
    public void Truncate_ReturnsOriginal_WhenUnderLimit()
    {
        var text = "Ethan Gray, Class of 2026 — Cloverleaf Track & Field.";
        SeoHelper.Truncate(text, 160).Should().Be(text);
    }

    [Fact]
    public void Truncate_NeverExceedsMaxLength()
    {
        var text = new string('a', 50) + " " + new string('b', 50) + " " + new string('c', 100);
        var result = SeoHelper.Truncate(text, 160);
        result.Length.Should().BeLessThanOrEqualTo(160);
    }

    [Fact]
    public void Truncate_BreaksOnWordBoundary_AndAddsEllipsis()
    {
        var text = string.Join(" ", Enumerable.Repeat("word", 60)); // way over 160 chars
        var result = SeoHelper.Truncate(text, 160);

        result.Should().EndWith("…");
        result.Should().NotContain("  ");
        // Word boundary break: nothing but whole "word" tokens (plus ellipsis) should appear.
        result.TrimEnd('…').Trim().Split(' ').Should().OnlyContain(token => token == "word");
    }

    // -------------------------------------------------------------------------
    // JSON-LD builders — structurally valid, correct schema.org @type and required fields
    // -------------------------------------------------------------------------

    [Fact]
    public void BuildOrganizationJsonLd_ProducesValidOrganizationSchema()
    {
        var json = SeoHelper.BuildOrganizationJsonLd("https://cloverleaftrack.com");
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.GetProperty("@context").GetString().Should().Be("https://schema.org");
        root.GetProperty("@type").GetString().Should().Be("Organization");
        root.GetProperty("name").GetString().Should().Be("Cloverleaf Track & Field");
        root.GetProperty("url").GetString().Should().Be("https://cloverleaftrack.com/");
    }

    [Fact]
    public void BuildPersonJsonLd_ProducesValidPersonSchema_WithAwardsAndGender()
    {
        var json = SeoHelper.BuildPersonJsonLd(
            "Ethan Gray",
            "https://cloverleaftrack.com/roster/ethan-gray",
            "Ethan Gray, Class of 2026.",
            "Boys",
            new[] { "School Record — 400m: 52.40" });

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.GetProperty("@type").GetString().Should().Be("Person");
        root.GetProperty("name").GetString().Should().Be("Ethan Gray");
        root.GetProperty("gender").GetString().Should().Be("Male");
        root.GetProperty("award").EnumerateArray().Select(e => e.GetString())
            .Should().Contain("School Record — 400m: 52.40");
        root.GetProperty("memberOf").GetProperty("@type").GetString().Should().Be("SportsTeam");
    }

    [Fact]
    public void BuildPersonJsonLd_OmitsAward_WhenNoAwardsGiven()
    {
        var json = SeoHelper.BuildPersonJsonLd(
            "Jane Doe", "https://cloverleaftrack.com/roster/jane-doe", null, "Girls", Enumerable.Empty<string>());

        using var doc = JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("award", out _).Should().BeFalse();
        doc.RootElement.GetProperty("gender").GetString().Should().Be("Female");
    }

    [Fact]
    public void BuildSportsEventJsonLd_ProducesValidSportsEventSchema_WithLocation()
    {
        var json = SeoHelper.BuildSportsEventJsonLd(
            "2026 Heidelberg HS Qualifier",
            new DateTime(2026, 1, 31),
            "https://cloverleaftrack.com/meets/2026-heidelberg-hs-qualifier",
            "Heidelberg University",
            "Tiffin",
            "OH");

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.GetProperty("@type").GetString().Should().Be("SportsEvent");
        root.GetProperty("name").GetString().Should().Be("2026 Heidelberg HS Qualifier");
        root.GetProperty("startDate").GetString().Should().Be("2026-01-31");

        var location = root.GetProperty("location");
        location.GetProperty("@type").GetString().Should().Be("Place");
        location.GetProperty("name").GetString().Should().Be("Heidelberg University");
        location.GetProperty("address").GetProperty("addressLocality").GetString().Should().Be("Tiffin");
    }

    [Fact]
    public void BuildSportsEventJsonLd_OmitsLocation_WhenNoLocationDataGiven()
    {
        var json = SeoHelper.BuildSportsEventJsonLd(
            "Some Meet", new DateTime(2026, 3, 1), "https://cloverleaftrack.com/meets/some-meet", null, null, null);

        using var doc = JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("location", out _).Should().BeFalse();
    }

    [Fact]
    public void BuildBreadcrumbJsonLd_ProducesOrderedListItems()
    {
        var breadcrumbs = new List<SeoBreadcrumbViewModel>
        {
            new() { Name = "Home", Path = "/" },
            new() { Name = "Roster", Path = "/roster" },
            new() { Name = "Ethan Gray", Path = "/roster/ethan-gray" }
        };

        var json = SeoHelper.BuildBreadcrumbJsonLd(breadcrumbs, "https://cloverleaftrack.com");
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.GetProperty("@type").GetString().Should().Be("BreadcrumbList");

        var items = root.GetProperty("itemListElement").EnumerateArray().ToList();
        items.Should().HaveCount(3);
        items[0].GetProperty("position").GetInt32().Should().Be(1);
        items[0].GetProperty("item").GetString().Should().Be("https://cloverleaftrack.com/");
        items[2].GetProperty("position").GetInt32().Should().Be(3);
        items[2].GetProperty("name").GetString().Should().Be("Ethan Gray");
        items[2].GetProperty("item").GetString().Should().Be("https://cloverleaftrack.com/roster/ethan-gray");
    }
}
