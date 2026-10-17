using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models.Enums;
using Slugify;

namespace CloverleafTrack.Services;

public interface IMeetSummaryLinkService
{
    /// <summary>HTML-encoded summary with athlete and event names linked, using the athletes/events that appear in this meet.</summary>
    Task<string> LinkifyAsync(int meetId, string text);
}

public class MeetSummaryLinkService(IMeetRepository meetRepository) : IMeetSummaryLinkService
{
    public async Task<string> LinkifyAsync(int meetId, string text)
    {
        var performances = await meetRepository.GetPerformancesForMeetAsync(meetId);
        var slugs = new SlugHelper();

        var athletes = new Dictionary<string, LinkableAthlete>(StringComparer.Ordinal);
        foreach (var p in performances.OrderBy(p => p.EventGender == Gender.Mixed)) // mixed relays last, so single-gender events set the gender
        {
            var members = p.AthleteId.HasValue
                ? new[] { p.AthleteName }
                : p.AthleteName.Split("|~|", StringSplitOptions.RemoveEmptyEntries);
            foreach (var name in members)
            {
                var gender = p.EventGender == Gender.Mixed ? (Gender?)null : p.EventGender;
                if (!athletes.TryGetValue(name, out var existing))
                    athletes[name] = new LinkableAthlete(name, slugs.GenerateSlug(name), gender);
                else if (existing.Gender == null && gender != null)
                    athletes[name] = existing with { Gender = gender };
            }
        }

        var events = performances
            .Where(p => !string.IsNullOrEmpty(p.EventKey))
            .Select(p => new LinkableEvent(p.EventName, p.EventKey, p.EventGender))
            .DistinctBy(e => e.Key)
            .ToList();

        return SummaryLinkifier.Linkify(text, athletes.Values.ToList(), events);
    }
}
