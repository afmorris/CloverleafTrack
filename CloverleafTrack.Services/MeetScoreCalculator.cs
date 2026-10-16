using CloverleafTrack.Models;
using CloverleafTrack.Models.Enums;

namespace CloverleafTrack.Services;

/// <summary>
/// Pure maths for MeetScore (BRAIN.md [C43], [C44]). Computed on read from raw marks; nothing is stored.
/// </summary>
public static class MeetScoreCalculator
{
    public const double PeakWeight = 0.45;
    public const double DepthWeight = 0.25;
    public const double ImprovementWeight = 0.30;

    public const int PeakCount = 5;
    public const int DepthCount = 40;

    /// <summary>Events with fewer recorded marks than this have unstable percentiles and are ignored for Peak/Depth.</summary>
    public const int MinEventMarkCount = 10;

    /// <summary>Seasons starting on/after this date have complete results (not just scoring performances). Seasons.Status is NOT reliable for this.</summary>
    public static readonly DateTime CompleteDataEraStart = new(2020, 12, 1);

    /// <summary>Prior-meet counts at or above this share one expected-PR-rate bucket.</summary>
    public const int MaxPriorMeetBucket = 6;

    public static bool IsCompleteEra(DateTime seasonStartDate) => seasonStartDate >= CompleteDataEraStart;

    public static Dictionary<int, MeetScoreResult> Calculate(IReadOnlyCollection<MeetScoreMark> marks)
    {
        var results = new Dictionary<int, MeetScoreResult>();
        var improvement = CalculateImprovement(marks);

        foreach (var meetMarks in marks.GroupBy(m => m.MeetId))
        {
            var first = meetMarks.First();
            var complete = IsCompleteEra(first.SeasonStartDate);
            improvement.TryGetValue(first.MeetId, out var imp);

            double? peak = null;
            double? depth = null;
            double? improvementScore = null;
            double score;
            MeetScoreBasis basis;

            if (!complete)
            {
                peak = Peak(meetMarks);
                if (peak == null) continue;
                score = peak.Value;
                basis = MeetScoreBasis.PeakOnly;
            }
            else if (first.MeetLevel == MeetLevel.JV)
            {
                improvementScore = imp?.Score ?? 50;
                score = improvementScore.Value;
                basis = MeetScoreBasis.ImprovementOnly;
            }
            else
            {
                peak = Peak(meetMarks);
                depth = Depth(meetMarks);
                if (peak == null || depth == null) continue;
                improvementScore = imp?.Score ?? 50;
                score = PeakWeight * peak.Value + DepthWeight * depth.Value + ImprovementWeight * improvementScore.Value;
                basis = MeetScoreBasis.Full;
            }

            results[first.MeetId] = new MeetScoreResult
            {
                MeetId = first.MeetId,
                Score = score,
                Basis = basis,
                Peak = peak,
                Depth = depth,
                Improvement = improvementScore,
                PRs = imp?.PRs ?? 0,
                ExpectedPRs = imp?.Expected ?? 0,
                EligibleAthleteEvents = imp?.Eligible ?? 0
            };
        }

        return results;
    }

    private static IEnumerable<MeetScoreMark> Scorable(IEnumerable<MeetScoreMark> marks) =>
        marks.Where(m => m.Percentile.HasValue && m.EventMarkCount >= MinEventMarkCount);

    /// <summary>Mean of the top 5 percentiles in the meet (any mark, relays included).</summary>
    public static double? Peak(IEnumerable<MeetScoreMark> meetMarks)
    {
        var top = Scorable(meetMarks).Select(m => (double)m.Percentile!.Value)
            .OrderByDescending(p => p).Take(PeakCount).ToList();
        return top.Count == 0 ? null : top.Average();
    }

    /// <summary>
    /// Mean of the 40 best athlete-event bests (best percentile per athlete per event; each relay performance counts once).
    /// Averaging every mark would punish large dual meets full of ordinary marks, so only the top 40 count.
    /// Meets with fewer than 40 athlete-events are averaged over what exists.
    /// </summary>
    public static double? Depth(IEnumerable<MeetScoreMark> meetMarks)
    {
        var bests = Scorable(meetMarks)
            .GroupBy(m => m.AthleteId.HasValue
                ? $"a{m.AthleteId}:{m.EventId}"
                : $"r{m.PerformanceId}")
            .Select(g => (double)g.Max(m => m.Percentile!.Value))
            .OrderByDescending(p => p).Take(DepthCount).ToList();
        return bests.Count == 0 ? null : bests.Average();
    }

    private sealed record ImprovementDetail(double Score, int PRs, double Expected, int Eligible);

    private readonly record struct Observation(int MeetId, int Bucket, bool IsPR);

    /// <summary>
    /// Improvement = 50 + 100*(PRs - ExpectedPRs)/(Eligible + 10), clamped 0-100. Individuals only.
    /// A PR is per athlete-event: the meet's best beats the best from strictly earlier meets. Only complete-era
    /// marks count as priors (earlier history is incomplete). Expected PRs use the empirical PR rate by number of
    /// prior meets, computed from the same data — early-season meets otherwise look unfairly good (61% PR rate on 1 prior vs 24% on 6+).
    /// </summary>
    private static Dictionary<int, ImprovementDetail> CalculateImprovement(IEnumerable<MeetScoreMark> marks)
    {
        var observations = new List<Observation>();

        var groups = marks
            .Where(m => m.AthleteId.HasValue && m.Value.HasValue && IsCompleteEra(m.SeasonStartDate))
            .GroupBy(m => (m.AthleteId, m.EventId));

        foreach (var g in groups)
        {
            var higherIsBetter = g.First().IsFieldEvent;
            var perMeet = g.GroupBy(m => m.MeetId)
                .Select(mg => (
                    MeetId: mg.Key,
                    Date: mg.First().MeetDate.Date,
                    Best: higherIsBetter ? mg.Max(m => m.Value!.Value) : mg.Min(m => m.Value!.Value)))
                .OrderBy(x => x.Date)
                .ToList();

            foreach (var meet in perMeet)
            {
                var earlier = perMeet.Where(x => x.Date < meet.Date).ToList();
                if (earlier.Count == 0) continue;
                var priorBest = higherIsBetter ? earlier.Max(x => x.Best) : earlier.Min(x => x.Best);
                var isPr = higherIsBetter ? meet.Best > priorBest : meet.Best < priorBest;
                observations.Add(new Observation(meet.MeetId, Math.Min(earlier.Count, MaxPriorMeetBucket), isPr));
            }
        }

        var overallRate = observations.Count == 0 ? 0 : observations.Average(o => o.IsPR ? 1.0 : 0.0);
        var rates = observations.GroupBy(o => o.Bucket)
            .ToDictionary(b => b.Key, b => b.Average(o => o.IsPR ? 1.0 : 0.0));

        return observations.GroupBy(o => o.MeetId).ToDictionary(
            g => g.Key,
            g =>
            {
                var prs = g.Count(o => o.IsPR);
                var expected = g.Sum(o => rates.GetValueOrDefault(o.Bucket, overallRate));
                var eligible = g.Count();
                var score = Math.Clamp(50 + 100 * (prs - expected) / (eligible + 10), 0, 100);
                return new ImprovementDetail(score, prs, expected, eligible);
            });
    }
}
