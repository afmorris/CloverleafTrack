using CloverleafTrack.DataAccess.Dtos;
using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models.Enums;
using CloverleafTrack.Services.Interfaces;
using CloverleafTrack.ViewModels.Scoring;
using Slugify;

namespace CloverleafTrack.Services;

public class ScoringService(
    ISeasonRepository seasonRepository,
    ISeasonScoringRepository seasonScoringRepository) : IScoringService
{
    public async Task<SeasonScoringViewModel?> GetSeasonScoringAsync(int seasonId)
    {
        var season = await seasonRepository.GetByIdAsync(seasonId);
        if (season == null || !season.ScoringEnabled)
            return null;

        var rows = await seasonScoringRepository.GetScoringDataForSeasonAsync(seasonId);

        var slugHelper = new SlugHelper();

        // Build per-athlete aggregations, grouped by gender
        var athleteMap = new Dictionary<(int AthleteId, Gender Gender), AthleteScoreRowViewModel>();

        // Track unique events per athlete for the 4-event limit
        var athleteEvents = new Dictionary<(int AthleteId, Gender Gender), HashSet<int>>();

        foreach (var row in rows)
        {
            var key = (row.AthleteId, row.AthleteGender);

            if (!athleteMap.TryGetValue(key, out var vm))
            {
                vm = new AthleteScoreRowViewModel
                {
                    AthleteId = row.AthleteId,
                    AthleteFirstName = row.AthleteFirstName,
                    AthleteLastName = row.AthleteLastName,
                    AthleteSlug = slugHelper.GenerateSlug($"{row.AthleteFirstName} {row.AthleteLastName}".Trim())
                };
                athleteMap[key] = vm;
                athleteEvents[key] = new HashSet<int>();
            }

            athleteEvents[key].Add(row.EventId);

            var isRelay = row.IsRelay;
            var isField = row.EventType is EventType.Field or EventType.FieldRelay
                                         or EventType.JumpRelay or EventType.ThrowsRelay;

            // ── Totals ──────────────────────────────────────────────
            vm.TotalFullPoints += row.FullPoints;
            vm.TotalSplitPoints += row.SplitPoints;

            // ── Running vs Field ────────────────────────────────────
            if (isField)
            {
                vm.FieldFullPoints += row.FullPoints;
                vm.FieldSplitPoints += row.SplitPoints;
            }
            else
            {
                vm.RunningFullPoints += row.FullPoints;
                vm.RunningSplitPoints += row.SplitPoints;
            }

            // ── Individual vs Relay ─────────────────────────────────
            if (isRelay)
            {
                vm.RelayFullPoints += row.FullPoints;
                vm.RelaySplitPoints += row.SplitPoints;
            }
            else
            {
                // Individual points are the same regardless of full/split mode
                vm.IndividualPoints += row.FullPoints;
            }

            // ── By Event Category ───────────────────────────────────
            if (!vm.FullPointsByCategory.ContainsKey(row.EventCategory))
            {
                vm.FullPointsByCategory[row.EventCategory] = 0;
                vm.SplitPointsByCategory[row.EventCategory] = 0;
            }
            vm.FullPointsByCategory[row.EventCategory] += row.FullPoints;
            vm.SplitPointsByCategory[row.EventCategory] += row.SplitPoints;
        }

        // Set event counts
        foreach (var (key, vm) in athleteMap)
            vm.EventCount = athleteEvents[key].Count;

        var boys = athleteMap
            .Where(kvp => kvp.Key.Gender == Gender.Male)
            .Select(kvp => kvp.Value)
            .OrderByDescending(v => v.TotalFullPoints)
            .ThenBy(v => v.AthleteLastName)
            .ToList();

        var girls = athleteMap
            .Where(kvp => kvp.Key.Gender == Gender.Female)
            .Select(kvp => kvp.Value)
            .OrderByDescending(v => v.TotalFullPoints)
            .ThenBy(v => v.AthleteLastName)
            .ToList();

        return new SeasonScoringViewModel
        {
            SeasonId = season.Id,
            SeasonName = season.Name,
            SeasonSlug = slugHelper.GenerateSlug(season.Name),
            Boys = boys,
            Girls = girls
        };
    }
}
