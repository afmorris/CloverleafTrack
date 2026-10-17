namespace CloverleafTrack.Services;

/// <summary>
/// Single source of truth for "what class was this athlete in when this performance happened."
/// Extracted from LeaderboardService (originally private) so AthleteService's career chart
/// (issue #26) can reuse the exact same August school-year rule instead of reimplementing it.
/// </summary>
public static class ClassYearCalculator
{
    /// <summary>
    /// Returns the athlete's class (Freshman/Sophomore/Junior/Senior) at the time the performance
    /// was set, based on the athlete's graduation year and the meet date. Returns null for relays
    /// or unknown graduation years. School year boundary: August or later means the school year
    /// that ends in meetDate.Year + 1.
    /// </summary>
    public static string? GetClassAtTimeOfPerformance(int? graduationYear, DateTime meetDate)
    {
        if (!graduationYear.HasValue) return null;
        // Meets in August or later belong to the school year that ends the following June
        var schoolYearEnd = meetDate.Month >= 8 ? meetDate.Year + 1 : meetDate.Year;
        return (graduationYear.Value - schoolYearEnd) switch
        {
            0 => "Senior",
            1 => "Junior",
            2 => "Sophomore",
            3 => "Freshman",
            _ => null
        };
    }

    /// <summary>
    /// The school year that ends in <paramref name="graduationYear"/>: August 1 of the previous year up to (not including)
    /// August 1 of the graduation year. Same boundary as <see cref="GetClassAtTimeOfPerformance"/>, so a class's senior
    /// season is exactly the meets for which that method returns "Senior".
    /// </summary>
    public static (DateTime StartInclusive, DateTime EndExclusive) GetSeniorSchoolYear(int graduationYear) =>
        (new DateTime(graduationYear - 1, 8, 1), new DateTime(graduationYear, 8, 1));
}
