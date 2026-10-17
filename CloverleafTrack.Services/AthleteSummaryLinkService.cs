using System.Net;
using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Services.Interfaces;
using Slugify;

namespace CloverleafTrack.Services;

public interface IAthleteSummaryLinkService
{
    /// <summary>HTML-encoded summary for the admin review page, loading the athlete's details itself.</summary>
    Task<string> LinkifyAsync(int athleteId, string text, string? linkClass = null);
}

public class AthleteSummaryLinkService(
    IAdminAthleteRepository athleteRepository,
    IAthleteService athleteService,
    IAthleteSummaryLinker linker) : IAthleteSummaryLinkService
{
    public async Task<string> LinkifyAsync(int athleteId, string text, string? linkClass = null)
    {
        var athlete = await athleteRepository.GetByIdAsync(athleteId);
        if (athlete == null) return WebUtility.HtmlEncode(text);

        var slug = new SlugHelper().GenerateSlug($"{athlete.FirstName}-{athlete.LastName}");
        var details = await athleteService.GetAthleteDetailsAsync(slug, athlete.GraduationYear);
        return details == null ? WebUtility.HtmlEncode(text) : await linker.LinkifyAsync(details, text, linkClass);
    }
}
