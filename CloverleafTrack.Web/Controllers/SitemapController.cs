using System.Text;
using CloverleafTrack.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CloverleafTrack.Web.Controllers;

/// <summary>
/// Serves /sitemap.xml, enumerating every canonical public route: home, the roster/leaderboard/meets/seasons
/// index pages, and every athlete, event, meet, and season detail page. Reuses ISearchService (already
/// enumerates athletes/meets/events for the site search overlay) and ISeasonService rather than adding new
/// repository queries. robots.txt (wwwroot/robots.txt) points crawlers at this route.
/// </summary>
public class SitemapController(ISearchService searchService, ISeasonService seasonService) : Controller
{
    [HttpGet("/sitemap.xml")]
    [ResponseCache(Duration = 3600, VaryByHeader = "Accept-Encoding")]
    public async Task<IActionResult> Index()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";

        var paths = new List<string>
        {
            "/",
            "/roster",
            "/leaderboard",
            "/meets",
            "/seasons"
        };

        var searchRecords = await searchService.GetSearchIndexAsync();
        paths.AddRange(searchRecords.Select(r => r.Url));

        var seasons = await seasonService.GetSeasonCardsAsync();
        paths.AddRange(seasons.Select(s => $"/seasons/{Uri.EscapeDataString(s.Name)}"));

        var sb = new StringBuilder();
        sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n");
        sb.Append("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">\n");
        foreach (var path in paths.Distinct())
        {
            sb.Append("  <url><loc>").Append(XmlEscape(baseUrl + path)).Append("</loc></url>\n");
        }
        sb.Append("</urlset>\n");

        return Content(sb.ToString(), "application/xml", Encoding.UTF8);
    }

    private static string XmlEscape(string value) => value
        .Replace("&", "&amp;")
        .Replace("<", "&lt;")
        .Replace(">", "&gt;")
        .Replace("\"", "&quot;")
        .Replace("'", "&apos;");
}
