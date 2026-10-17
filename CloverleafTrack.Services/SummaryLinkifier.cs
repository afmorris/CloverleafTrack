using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using CloverleafTrack.Models.Enums;

namespace CloverleafTrack.Services;

public record LinkableAthlete(string FullName, string Slug, Gender? Gender);

/// <param name="Name">Display name as it appears in text, e.g. "4x400M Relay".</param>
/// <param name="Key">Event key as used in /events/{key}, e.g. BOYS_OUTDOOR_4X400M_RELAY.</param>
public record LinkableEvent(string Name, string Key, Gender Gender);

/// <summary>
/// Turns plain summary text into HTML with athlete names linked to /roster/{slug} and event names linked to
/// /events/{key}. Pure and stateless so it can be unit tested; the stored summary text is never modified.
/// Event names exist per gender, so each mention is resolved from context: an adjacent "boys"/"girls"/"mixed",
/// else the nearest one earlier in the sentence, else the only gender the meet has that event for, else the
/// gender of the nearest athlete named earlier in the sentence. Unresolvable mentions are left as plain text.
/// </summary>
public static class SummaryLinkifier
{
    public static string Linkify(string text, IReadOnlyList<LinkableAthlete> athletes, IReadOnlyList<LinkableEvent> events)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;

        // Phrase -> athlete. Full names always; a bare surname only when it identifies exactly one athlete in the meet.
        var athleteByPhrase = new Dictionary<string, LinkableAthlete>(StringComparer.Ordinal);
        foreach (var a in athletes) athleteByPhrase.TryAdd(a.FullName, a);
        foreach (var group in athletes.GroupBy(a => Surname(a.FullName)))
        {
            var distinct = group.Select(a => a.Slug).Distinct().ToList();
            if (distinct.Count == 1 && group.Key.Length > 0) athleteByPhrase.TryAdd(group.Key, group.First());
        }

        // Phrase (lower-case) -> events with that name. A relay may also be written without the word "Relay" ("4x100M").
        var eventsByPhrase = new Dictionary<string, List<LinkableEvent>>(StringComparer.OrdinalIgnoreCase);
        var aliasPhrases = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var e in events)
        {
            AddEvent(eventsByPhrase, e.Name, e);
            if (e.Name.EndsWith(" Relay", StringComparison.OrdinalIgnoreCase))
            {
                var alias = e.Name[..^" Relay".Length];
                AddEvent(eventsByPhrase, alias, e);
                aliasPhrases.Add(alias);
            }
        }

        var alternatives = athleteByPhrase.Keys.Concat(eventsByPhrase.Keys)
            .Distinct(StringComparer.Ordinal)
            .OrderByDescending(p => p.Length)
            .Select(p => aliasPhrases.Contains(p) && !athleteByPhrase.ContainsKey(p)
                // bare relay alias: only when it reads like a list item ("4x100M, 4x200M and 4x400M relays")
                ? Regex.Escape(p) + @"(?=,|\s+and\b|\s+relays?\b)"
                : Regex.Escape(p));
        var pattern = alternatives.Any() ? @"(?<![\w])(?:" + string.Join("|", alternatives) + @")(?![\w])" : null;
        if (pattern == null) return WebUtility.HtmlEncode(text);

        var regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        var sb = new StringBuilder();
        var pos = 0;
        Gender? lastAthleteGender = null;
        var lastAthletePos = -1;
        var lastSentenceStart = 0;

        foreach (Match m in regex.Matches(text))
        {
            sb.Append(WebUtility.HtmlEncode(text[pos..m.Index]));
            pos = m.Index + m.Length;
            var sentenceStart = SentenceStart(text, m.Index);
            if (sentenceStart != lastSentenceStart) { lastAthleteGender = null; lastAthletePos = -1; lastSentenceStart = sentenceStart; }

            if (athleteByPhrase.TryGetValue(m.Value, out var athlete))
            {
                if (athlete.Gender != null) { lastAthleteGender = athlete.Gender; lastAthletePos = m.Index; }
                sb.Append($"<a href=\"/roster/{WebUtility.HtmlEncode(athlete.Slug)}\">{WebUtility.HtmlEncode(m.Value)}</a>");
            }
            else if (eventsByPhrase.TryGetValue(m.Value, out var candidates) &&
                     Resolve(candidates, text, m.Index, sentenceStart, lastAthleteGender, lastAthletePos) is { } evt)
            {
                sb.Append($"<a href=\"/events/{WebUtility.HtmlEncode(evt.Key.ToLowerInvariant())}\">{WebUtility.HtmlEncode(m.Value)}</a>");
            }
            else
            {
                sb.Append(WebUtility.HtmlEncode(m.Value));
            }
        }

        sb.Append(WebUtility.HtmlEncode(text[pos..]));
        return sb.ToString();
    }

    private static void AddEvent(Dictionary<string, List<LinkableEvent>> map, string phrase, LinkableEvent e)
    {
        if (!map.TryGetValue(phrase, out var list)) map[phrase] = list = new List<LinkableEvent>();
        if (!list.Any(x => x.Key == e.Key)) list.Add(e);
    }

    private static LinkableEvent? Resolve(List<LinkableEvent> candidates, string text, int index, int sentenceStart,
        Gender? athleteGender, int athletePos)
    {
        if (candidates.Count == 1) return candidates[0]; // the meet has this event for only one gender

        // Whichever cue sits closer to the event wins: "the girls 100M" (gender word right before it), or
        // "..., and Amelia Ryncarz (discus)" where a named athlete is nearer than an earlier "boys".
        var (word, wordPos) = GenderWordBefore(text, index, sentenceStart);
        Gender? wanted = wordPos >= athletePos ? word ?? athleteGender : athleteGender ?? word;
        return wanted is { } g ? candidates.FirstOrDefault(c => c.Gender == g) : null;
    }

    /// <summary>The nearest "boys"/"girls"/"mixed" earlier in the same sentence, with its position (-1 when none).</summary>
    private static (Gender? Gender, int Position) GenderWordBefore(string text, int index, int sentenceStart)
    {
        var matches = Regex.Matches(text[sentenceStart..index], @"\b(boys|girls|mixed)\b", RegexOptions.IgnoreCase);
        if (matches.Count == 0) return (null, -1);
        var last = matches[^1];
        var gender = last.Value.ToLowerInvariant() switch
        {
            "boys" => Gender.Male,
            "girls" => Gender.Female,
            _ => Gender.Mixed
        };
        return (gender, sentenceStart + last.Index);
    }

    private static int SentenceStart(string text, int index)
    {
        var i = text.LastIndexOf(". ", index, StringComparison.Ordinal);
        return i < 0 ? 0 : i + 2;
    }

    private static string Surname(string fullName)
    {
        var i = fullName.LastIndexOf(' ');
        return i < 0 ? string.Empty : fullName[(i + 1)..];
    }
}
