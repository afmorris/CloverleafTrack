using CloverleafTrack.Models.Enums;
using CloverleafTrack.Services;
using FluentAssertions;

namespace CloverleafTrack.Tests.Unit.Services;

public class SummaryLinkifierTests
{
    private static readonly LinkableAthlete[] Athletes =
    {
        new("Grady Pamer", "grady-pamer", Gender.Male),
        new("Abigail Reinhardt", "abigail-reinhardt", Gender.Female),
        new("Adam Gray", "adam-gray", Gender.Male),
        new("Ethan Gray", "ethan-gray", Gender.Male),
    };

    private static readonly LinkableEvent[] Events =
    {
        new("100M", "BOYS_OUTDOOR_100M", Gender.Male),
        new("100M", "GIRLS_OUTDOOR_100M", Gender.Female),
        new("100M Hurdles", "GIRLS_OUTDOOR_100M_HURDLES", Gender.Female),
        new("4x400M Relay", "BOYS_OUTDOOR_4X400M_RELAY", Gender.Male),
        new("4x400M Relay", "GIRLS_OUTDOOR_4X400M_RELAY", Gender.Female),
        new("Discus", "BOYS_OUTDOOR_DISCUS", Gender.Male),
    };

    private static string Run(string text) => SummaryLinkifier.Linkify(text, Athletes, Events);

    [Fact]
    public void Links_full_names_and_unique_surnames_but_not_ambiguous_ones()
    {
        var html = Run("Grady Pamer won. Pamer's mark stood, as did Reinhardt. Gray was close.");

        html.Should().Contain("<a href=\"/roster/grady-pamer\">Grady Pamer</a>");
        html.Should().Contain("<a href=\"/roster/grady-pamer\">Pamer</a>'s".Replace("'", "&#39;"));
        html.Should().Contain("<a href=\"/roster/abigail-reinhardt\">Reinhardt</a>");
        html.Should().NotContain("href=\"/roster/adam-gray\">Gray").And.NotContain("ethan-gray\">Gray"); // two Grays in the meet
    }

    [Fact]
    public void Adjacent_gender_word_picks_the_event_and_longest_name_wins()
    {
        var html = Run("She won the girls 100M and the girls 100M Hurdles; he won the boys 100M.");

        html.Should().Contain("<a href=\"/events/girls_outdoor_100m\">100M</a> and the girls <a href=\"/events/girls_outdoor_100m_hurdles\">100M Hurdles</a>");
        html.Should().Contain("<a href=\"/events/boys_outdoor_100m\">100M</a>.");
    }

    [Fact]
    public void Gender_carries_across_a_list_and_falls_back_to_the_nearest_athlete()
    {
        Run("The girls won the 100M and the 4x400M relay.")
            .Should().Contain("girls_outdoor_100m\">100M").And.Contain("girls_outdoor_4x400m_relay\">4x400M relay");
        Run("Grady Pamer ran 10.9 in the 100M.").Should().Contain("boys_outdoor_100m\">100M");
    }

    [Fact]
    public void A_nearer_athlete_beats_an_earlier_gender_word()
    {
        // "boys" appears earlier in the sentence, but Abigail Reinhardt (a girl) is the closest cue to the event
        Run("The boys won, and Abigail Reinhardt (100M) was close.").Should().Contain("girls_outdoor_100m\">100M");
    }

    [Fact]
    public void Single_gender_event_links_without_context_and_ambiguous_event_stays_plain()
    {
        Run("Someone threw the discus.").Should().Contain("/events/boys_outdoor_discus\">discus</a>");
        Run("Someone ran the 100M.").Should().NotContain("<a ");
    }

    [Fact]
    public void Bare_relay_names_in_a_list_link_and_text_is_html_encoded()
    {
        var html = Run("The boys won the 4x400M and the girls <b>won</b> too.");

        html.Should().Contain("boys_outdoor_4x400m_relay\">4x400M</a>");
        html.Should().Contain("&lt;b&gt;won&lt;/b&gt;").And.NotContain("<b>");
    }
}
