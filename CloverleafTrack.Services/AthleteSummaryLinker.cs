using System.Net;
using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.ViewModels.Athletes;
using Slugify;

namespace CloverleafTrack.Services;

public interface IAthleteSummaryLinker
{
    /// <summary>HTML-encoded summary with the athlete, teammates, meets and events named in the text linked.</summary>
    Task<string> LinkifyAsync(AthleteDetailsViewModel details, string text, string? linkClass = null);
}

/// <summary>Builds the athlete's link context (own and relay-teammate names, meets, events with keys) from the athlete page's own view model.</summary>
public class AthleteSummaryLinker(IAdminEventRepository eventRepository) : IAthleteSummaryLinker
{
    public async Task<string> LinkifyAsync(AthleteDetailsViewModel details, string text, string? linkClass = null)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;

        var slugs = new SlugHelper();
        var eventIds = details.PersonalRecords.Select(r => r.EventId)
            .Concat(details.Seasons.SelectMany(s => s.EventGroups).Select(g => g.EventId))
            .Concat(details.CareerCharts.Select(c => c.EventId))
            .ToHashSet();
        var events = (await eventRepository.GetAllAsync())
            .Where(e => eventIds.Contains(e.Id) && !string.IsNullOrEmpty(e.EventKey))
            .Select(e => new LinkableEvent(e.Name, e.EventKey, e.Gender ?? details.Gender, e.Environment))
            .ToList();

        var performances = details.Seasons.SelectMany(s => s.EventGroups).SelectMany(g => g.Performances).ToList();
        var meets = details.PersonalRecords.Select(r => r.MeetName)
            .Concat(performances.Select(p => p.MeetName))
            .Where(n => !string.IsNullOrWhiteSpace(n)).Distinct()
            .Select(n => new LinkableMeet(n, slugs.GenerateSlug(n))).ToList();

        var ownSlug = slugs.GenerateSlug($"{details.FirstName}-{details.LastName}");
        var athletes = new List<LinkableAthlete> { new(details.FullName, ownSlug, details.Gender) };
        athletes.AddRange(details.PersonalRecords.SelectMany(r => r.RelayMembers)
            .Concat(performances.SelectMany(p => p.RelayMembers))
            .Where(n => n != details.FullName).Distinct()
            .Select(n => new LinkableAthlete(n, slugs.GenerateSlug(n), null)));

        return SummaryLinkifier.Linkify(text, athletes, events, linkClass, meets, details.Gender);
    }
}
