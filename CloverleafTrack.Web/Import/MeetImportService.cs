using CloverleafTrack.DataAccess.Dtos;
using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models.Enums;

namespace CloverleafTrack.Web.Import;

/// <summary>Loads a meet's current state for <see cref="MeetImportPlanner"/> and writes the rows the admin approved. BRAIN.md [C122].</summary>
public class MeetImportService(
    IAdminMeetRepository meetRepository,
    IAdminEventRepository eventRepository,
    IAdminAthleteRepository athleteRepository,
    IAdminMeetParticipantRepository participantRepository,
    IAdminMeetEntryRepository entryRepository,
    IAdminPerformanceRepository performanceRepository,
    IAdminMeetPlacingRepository placingRepository,
    IAdminMeetImportRepository importRepository)
{
    public async Task<MeetImportContext?> LoadContextAsync(int meetId)
    {
        var meet = await meetRepository.GetByIdWithDetailsAsync(meetId);
        if (meet == null) return null;

        var performances = await performanceRepository.GetPerformancesForMeetAsync(meetId);
        var existing = new List<ExistingPerformance>();
        foreach (var p in performances)
        {
            existing.Add(new ExistingPerformance
            {
                Id = p.Id,
                EventId = p.EventId,
                AthleteId = p.AthleteId,
                RelayAthleteIds = p.AthleteId.HasValue ? new List<int>() : await performanceRepository.GetAthleteIdsForPerformanceAsync(p.Id),
                Status = p.ResultStatus,
                TimeSeconds = p.TimeSeconds,
                DistanceInches = p.DistanceInches
            });
        }

        return new MeetImportContext
        {
            Meet = meet,
            Events = await eventRepository.GetByGenderAndEnvironmentAsync(null, meet.Environment),
            Athletes = await athleteRepository.GetAllAsync(),
            Participants = await participantRepository.GetForMeetAsync(meetId),
            Entries = await entryRepository.GetForMeetAsync(meetId),
            Performances = existing,
            Placings = await placingRepository.GetForMeetAsync(meetId)
        };
    }

    public async Task<MeetImportOutcome> ApplyAsync(MeetImportContext ctx, ImportPlan plan, IReadOnlySet<int> chosenRows)
    {
        var writes = new List<MeetImportWrite>();
        foreach (var row in plan.Rows.Where(r => r.CanApply && chosenRows.Contains(r.Index)))
        {
            var write = new MeetImportWrite
            {
                EventId = row.EventId!.Value,
                IsRelay = row.IsRelay,
                AthleteId = row.IsRelay ? null : row.AthleteId,
                RelayAthleteIds = row.RelayAthleteIds,
                CreatesResult = row.Kind == ImportRowKind.Result,
                Status = row.Status,
                TimeSeconds = row.TimeSeconds,
                DistanceInches = row.DistanceInches,
                ExistingPerformanceId = row.ExistingPerformanceId,
                ReplaceMark = row.State == ImportRowState.Differs
            };

            foreach (var p in row.Placings)
            {
                // Points come from the same scoring-template lookup the single-result entry screen uses (seated events and dual relays handled there).
                var full = await placingRepository.GetTemplatePointsAsync(ctx.Meet.Id, row.EventId.Value, p.Place);
                var split = row.IsRelay && row.AthleteCount > 1 ? full / row.AthleteCount : full;
                write.Placings.Add(new MeetImportPlacingWrite { MeetParticipantId = p.MeetParticipantId, Place = p.Place, FullPoints = full, SplitPoints = split });
            }
            writes.Add(write);
        }

        if (writes.Count == 0) return new MeetImportOutcome();
        return await importRepository.ApplyAsync(ctx.Meet.Id, writes);
    }
}
