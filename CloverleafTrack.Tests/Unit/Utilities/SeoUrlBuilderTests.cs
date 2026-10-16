using CloverleafTrack.Web.Utilities;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace CloverleafTrack.Tests.Unit.Utilities;

public class SeoUrlBuilderTests
{
    private static SeoUrlBuilder Create(string? baseUrl, bool htmlExtension, HttpContext? context = null) =>
        new(Options.Create(new SeoUrlOptions { CanonicalBaseUrl = baseUrl, HtmlExtension = htmlExtension }),
            new HttpContextAccessor { HttpContext = context });

    [Fact]
    public void PageUrl_Home_IsBaseWithTrailingSlash_EvenWithHtmlExtension()
    {
        Create("https://cloverleaftrack.com", true).PageUrl("/").Should().Be("https://cloverleaftrack.com/");
    }

    [Theory]
    [InlineData("/roster/ethan-gray", "https://cloverleaftrack.com/roster/ethan-gray.html")]
    [InlineData("/roster", "https://cloverleaftrack.com/roster.html")]
    [InlineData("/meets/", "https://cloverleaftrack.com/meets.html")]
    [InlineData("events/boys_indoor_1600m", "https://cloverleaftrack.com/events/boys_indoor_1600m.html")]
    [InlineData("/meets/x.html", "https://cloverleaftrack.com/meets/x.html")]
    public void PageUrl_WithHtmlExtension_AppendsHtmlOnce(string path, string expected)
    {
        Create("https://cloverleaftrack.com/", true).PageUrl(path).Should().Be(expected);
    }

    [Fact]
    public void PageUrl_WithoutHtmlExtension_LeavesPathUntouched()
    {
        Create("https://cloverleaftrack.com", false).PageUrl("/roster/ethan-gray")
            .Should().Be("https://cloverleaftrack.com/roster/ethan-gray");
    }

    [Fact]
    public void AssetUrl_NeverGetsHtmlExtension()
    {
        Create("https://cloverleaftrack.com", true).AssetUrl("/img/hero-home.jpg")
            .Should().Be("https://cloverleaftrack.com/img/hero-home.jpg");
    }

    [Fact]
    public void BaseUrl_PrefersConfiguredOriginOverRequestHost()
    {
        var context = new DefaultHttpContext();
        context.Request.Scheme = "http";
        context.Request.Host = new HostString("ctf.morriscloud.com");

        Create("https://cloverleaftrack.com", true, context).BaseUrl.Should().Be("https://cloverleaftrack.com");
    }

    [Fact]
    public void BaseUrl_FallsBackToRequestOrigin_WhenNotConfigured()
    {
        var context = new DefaultHttpContext();
        context.Request.Scheme = "https";
        context.Request.Host = new HostString("localhost:5001");

        Create(null, false, context).PageUrl("/meets").Should().Be("https://localhost:5001/meets");
    }
}
