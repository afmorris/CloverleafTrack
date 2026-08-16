namespace CloverleafTrack.ViewModels.Shared;

/// <summary>
/// Per-page SEO data. Views populate this from data they already have and stash it on
/// ViewData["Seo"]; _Layout.cshtml renders it once via the _SeoMetadata partial.
/// Any field left unset falls back to a sensible sitewide default in the partial.
/// </summary>
public class SeoMetadataViewModel
{
    /// <summary>Data-derived page description, must be kept under ~160 characters (see SeoHelper.Truncate).</summary>
    public string? Description { get; set; }

    /// <summary>Relative path (e.g. "/roster/ethan-gray") used for canonical link + og:url. Defaults to the current request path when null.</summary>
    public string? CanonicalPath { get; set; }

    /// <summary>og:type value — "website", "profile", or "article", etc. Defaults to "website".</summary>
    public string OgType { get; set; } = "website";

    /// <summary>Relative path to the OG/Twitter image. Defaults to the sitewide fallback image when null (see BRAIN.md — per-page OG image generation is a follow-up).</summary>
    public string? ImagePath { get; set; }

    /// <summary>Breadcrumb trail for the sitewide BreadcrumbList JSON-LD. Defaults to just "Home" when empty.</summary>
    public List<SeoBreadcrumbViewModel> Breadcrumbs { get; set; } = new();

    /// <summary>Additional pre-serialized JSON-LD objects (Person / SportsEvent / Organization) — each entry is the JSON for one &lt;script type="application/ld+json"&gt; block. Build these with SeoHelper.</summary>
    public List<string> JsonLdBlocks { get; set; } = new();
}

public class SeoBreadcrumbViewModel
{
    public string Name { get; set; } = string.Empty;

    /// <summary>Relative path, e.g. "/roster".</summary>
    public string Path { get; set; } = string.Empty;
}
