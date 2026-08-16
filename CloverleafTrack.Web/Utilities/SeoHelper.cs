using System.Text.Json;
using CloverleafTrack.ViewModels.Shared;

namespace CloverleafTrack.Web.Utilities;

/// <summary>
/// Static helpers for building SEO metadata: safe description truncation and
/// schema.org JSON-LD serialization for Person, SportsEvent, Organization, and
/// BreadcrumbList. See BRAIN.md for the overall SEO/OG/JSON-LD design.
/// </summary>
public static class SeoHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = false };

    /// <summary>
    /// Truncates to at most maxLength characters, breaking on a word boundary where
    /// possible and appending an ellipsis. Safety net — callers should already be
    /// producing descriptions under the limit from real page data.
    /// </summary>
    public static string Truncate(string? text, int maxLength = 160)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        if (text.Length <= maxLength)
        {
            return text;
        }

        var truncated = text.Substring(0, maxLength - 1);
        var lastSpace = truncated.LastIndexOf(' ');
        if (lastSpace > 40)
        {
            truncated = truncated.Substring(0, lastSpace);
        }

        return truncated.TrimEnd(',', '.', ';', ':', ' ') + "…";
    }

    public static string BuildOrganizationJsonLd(string baseUrl)
    {
        var obj = new Dictionary<string, object?>
        {
            ["@context"] = "https://schema.org",
            ["@type"] = "Organization",
            ["name"] = "Cloverleaf Track & Field",
            ["url"] = baseUrl + "/",
            ["logo"] = baseUrl + "/img/hero-home.jpg",
            ["sameAs"] = new[] { "https://www.coltsathletics.org/" }
        };

        return JsonSerializer.Serialize(obj, JsonOptions);
    }

    public static string BuildPersonJsonLd(string name, string url, string? description, string? genderLabel, IEnumerable<string>? awards = null)
    {
        var obj = new Dictionary<string, object?>
        {
            ["@context"] = "https://schema.org",
            ["@type"] = "Person",
            ["name"] = name,
            ["url"] = url
        };

        if (!string.IsNullOrWhiteSpace(description))
        {
            obj["description"] = description;
        }

        if (genderLabel == "Boys")
        {
            obj["gender"] = "Male";
        }
        else if (genderLabel == "Girls")
        {
            obj["gender"] = "Female";
        }

        var awardList = awards?.Where(a => !string.IsNullOrWhiteSpace(a)).ToList();
        if (awardList is { Count: > 0 })
        {
            obj["award"] = awardList;
        }

        obj["memberOf"] = new Dictionary<string, object?>
        {
            ["@type"] = "SportsTeam",
            ["name"] = "Cloverleaf Colts Track & Field",
            ["sport"] = "Track and Field"
        };

        return JsonSerializer.Serialize(obj, JsonOptions);
    }

    public static string BuildSportsEventJsonLd(string name, DateTime startDate, string url, string? locationName, string? locationCity, string? locationState)
    {
        var obj = new Dictionary<string, object?>
        {
            ["@context"] = "https://schema.org",
            ["@type"] = "SportsEvent",
            ["name"] = name,
            ["startDate"] = startDate.ToString("yyyy-MM-dd"),
            ["url"] = url,
            ["sport"] = "Track and Field"
        };

        if (!string.IsNullOrWhiteSpace(locationName) || !string.IsNullOrWhiteSpace(locationCity))
        {
            var address = new Dictionary<string, object?> { ["@type"] = "PostalAddress" };
            if (!string.IsNullOrWhiteSpace(locationCity))
            {
                address["addressLocality"] = locationCity;
            }
            if (!string.IsNullOrWhiteSpace(locationState))
            {
                address["addressRegion"] = locationState;
            }

            obj["location"] = new Dictionary<string, object?>
            {
                ["@type"] = "Place",
                ["name"] = string.IsNullOrWhiteSpace(locationName) ? locationCity : locationName,
                ["address"] = address
            };
        }

        return JsonSerializer.Serialize(obj, JsonOptions);
    }

    public static string BuildBreadcrumbJsonLd(IReadOnlyList<SeoBreadcrumbViewModel> breadcrumbs, string baseUrl)
    {
        var items = breadcrumbs.Select((b, i) => new Dictionary<string, object?>
        {
            ["@type"] = "ListItem",
            ["position"] = i + 1,
            ["name"] = b.Name,
            ["item"] = baseUrl + b.Path
        }).ToList();

        var obj = new Dictionary<string, object?>
        {
            ["@context"] = "https://schema.org",
            ["@type"] = "BreadcrumbList",
            ["itemListElement"] = items
        };

        return JsonSerializer.Serialize(obj, JsonOptions);
    }
}
