using Microsoft.Extensions.Options;

namespace CloverleafTrack.Web.Utilities;

/// <summary>Bound from the "Seo" configuration section.</summary>
public class SeoUrlOptions
{
    /// <summary>
    /// Public origin the pages are served from, e.g. "https://cloverleaftrack.com" (no trailing slash).
    /// When empty, the current request's scheme and host are used.
    /// </summary>
    public string? CanonicalBaseUrl { get; set; }

    /// <summary>
    /// True when the public site is the static export, where every page is saved as "&lt;path&gt;.html"
    /// (wget --html-extension) and the extensionless route 404s. Canonical URLs must name the URL that resolves.
    /// </summary>
    public bool HtmlExtension { get; set; }
}

/// <summary>
/// Builds the absolute, public URLs used in canonical links, og:url/og:image, sitemap and JSON-LD.
/// The app is crawled on an internal host (ctf.morriscloud.com), so the request host must never leak into these.
/// </summary>
public interface ISeoUrlBuilder
{
    /// <summary>Public origin with no trailing slash.</summary>
    string BaseUrl { get; }

    /// <summary>Absolute URL of a page route such as "/roster/ethan-gray" (or "/" for home).</summary>
    string PageUrl(string path);

    /// <summary>Absolute URL of a static asset such as "/img/hero-home.jpg" — never gets an extension added.</summary>
    string AssetUrl(string path);
}

public class SeoUrlBuilder(IOptions<SeoUrlOptions> options, IHttpContextAccessor httpContextAccessor) : ISeoUrlBuilder
{
    public string BaseUrl
    {
        get
        {
            var configured = options.Value.CanonicalBaseUrl?.Trim().TrimEnd('/');
            if (!string.IsNullOrEmpty(configured))
            {
                return configured;
            }

            var request = httpContextAccessor.HttpContext?.Request;
            return request is null ? string.Empty : $"{request.Scheme}://{request.Host}";
        }
    }

    public string PageUrl(string path)
    {
        var normalized = Normalize(path);
        if (normalized == "/")
        {
            return BaseUrl + "/";
        }

        normalized = normalized.TrimEnd('/');
        if (options.Value.HtmlExtension && !normalized.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
        {
            normalized += ".html";
        }

        return BaseUrl + normalized;
    }

    public string AssetUrl(string path) => BaseUrl + Normalize(path);

    private static string Normalize(string? path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return "/";
        }

        return path.StartsWith('/') ? path : "/" + path;
    }
}
