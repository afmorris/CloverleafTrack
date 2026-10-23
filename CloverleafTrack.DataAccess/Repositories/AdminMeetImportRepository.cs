using System.Data;
using CloverleafTrack.DataAccess.Dtos;
using CloverleafTrack.DataAccess.Interfaces;
using Dapper;

namespace CloverleafTrack.DataAccess.Repositories;

public class AdminMeetImportRepository(IDbConnectionFactory connectionFactory) : IAdminMeetImportRepository
{
    public async Task<MeetImportOutcome> ApplyAsync(int meetId, IReadOnlyList<MeetImportWrite> writes)
    {
        var outcome = new MeetImportOutcome();
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using (var tx = connection.BeginTransaction())
        {
            try
            {
                // Entry-only writes first so a result later in the same file links to them instead of creating a twin.
                foreach (var w in writes.Where(w => !w.CreatesResult))
                    await ApplyEntryOnlyAsync(connection, tx, meetId, w, outcome);
                foreach (var w in writes.Where(w => w.CreatesResult))
                    await ApplyResultAsync(connection, tx, meetId, w, outcome);
                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        await connection.ExecuteAsync("EXEC sp_RebuildLeaderboards");
        return outcome;
    }

    private static async Task ApplyEntryOnlyAsync(IDbConnection c, IDbTransaction tx, int meetId, MeetImportWrite w, MeetImportOutcome outcome)
    {
        if (await FindUnlinkedEntryAsync(c, tx, meetId, w) != null) return;
        await InsertEntryAsync(c, tx, meetId, w, performanceId: null);
        outcome.EntriesCreated++;
    }

    private static async Task ApplyResultAsync(IDbConnection c, IDbTransaction tx, int meetId, MeetImportWrite w, MeetImportOutcome outcome)
    {
        var performanceId = w.ExistingPerformanceId;
        if (performanceId == null)
        {
            performanceId = await c.ExecuteScalarAsync<int>(@"
                INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, ResultStatus)
                OUTPUT INSERTED.Id
                VALUES (@AthleteId, @MeetId, @EventId, @DistanceInches, @TimeSeconds, @ResultStatus)",
                new
                {
                    AthleteId = w.IsRelay ? (int?)null : w.AthleteId,
                    MeetId = meetId,
                    w.EventId,
                    w.DistanceInches,
                    w.TimeSeconds,
                    ResultStatus = (byte)w.Status
                }, tx);
            if (w.IsRelay)
            {
                await c.ExecuteAsync("INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@PerformanceId, @AthleteId)",
                    w.RelayAthleteIds.Select(id => new { PerformanceId = performanceId, AthleteId = id }), tx);
            }
            outcome.PerformancesCreated++;
        }
        else if (w.ReplaceMark)
        {
            await c.ExecuteAsync(@"
                UPDATE Performances
                SET DistanceInches = @DistanceInches, TimeSeconds = @TimeSeconds, ResultStatus = @ResultStatus
                WHERE Id = @Id",
                new { Id = performanceId, w.DistanceInches, w.TimeSeconds, ResultStatus = (byte)w.Status }, tx);
            outcome.MarksReplaced++;
        }

        var entryId = await FindUnlinkedEntryAsync(c, tx, meetId, w);
        if (entryId != null)
        {
            await c.ExecuteAsync("UPDATE MeetEntries SET PerformanceId = @PerformanceId WHERE Id = @Id AND PerformanceId IS NULL",
                new { PerformanceId = performanceId, Id = entryId }, tx);
            outcome.EntriesLinked++;
        }
        else if (!await HasEntryForPerformanceAsync(c, tx, performanceId.Value))
        {
            await InsertEntryAsync(c, tx, meetId, w, performanceId);
            outcome.EntriesCreated++;
        }

        foreach (var p in w.Placings)
        {
            await c.ExecuteAsync(@"
                DELETE FROM MeetPlacings
                WHERE PerformanceId = @PerformanceId
                  AND ((@ParticipantId IS NULL AND MeetParticipantId IS NULL) OR MeetParticipantId = @ParticipantId)",
                new { PerformanceId = performanceId, ParticipantId = p.MeetParticipantId }, tx);
            await c.ExecuteAsync(@"
                INSERT INTO MeetPlacings (MeetId, PerformanceId, MeetParticipantId, Place, FullPoints, SplitPoints)
                VALUES (@MeetId, @PerformanceId, @ParticipantId, @Place, @FullPoints, @SplitPoints)",
                new { MeetId = meetId, PerformanceId = performanceId, ParticipantId = p.MeetParticipantId, p.Place, p.FullPoints, p.SplitPoints }, tx);
            outcome.PlacingsWritten++;
        }
    }

    private static Task<bool> HasEntryForPerformanceAsync(IDbConnection c, IDbTransaction tx, int performanceId) =>
        c.ExecuteScalarAsync<bool>("SELECT CASE WHEN EXISTS (SELECT 1 FROM MeetEntries WHERE PerformanceId = @PerformanceId AND Deleted = 0) THEN 1 ELSE 0 END",
            new { PerformanceId = performanceId }, tx);

    /// <summary>A pre-meet entry with no result yet for the same event and athlete (individual) or athlete set (relay).</summary>
    private static async Task<int?> FindUnlinkedEntryAsync(IDbConnection c, IDbTransaction tx, int meetId, MeetImportWrite w)
    {
        if (!w.IsRelay)
        {
            return await c.QueryFirstOrDefaultAsync<int?>(@"
                SELECT TOP 1 Id FROM MeetEntries
                WHERE MeetId = @MeetId AND EventId = @EventId AND AthleteId = @AthleteId AND PerformanceId IS NULL AND Deleted = 0
                ORDER BY Id", new { MeetId = meetId, w.EventId, w.AthleteId }, tx);
        }

        var count = w.RelayAthleteIds.Count;
        return await c.QueryFirstOrDefaultAsync<int?>(@"
            SELECT TOP 1 me.Id FROM MeetEntries me
            WHERE me.MeetId = @MeetId AND me.EventId = @EventId AND me.AthleteId IS NULL AND me.PerformanceId IS NULL AND me.Deleted = 0
              AND (SELECT COUNT(*) FROM MeetEntryAthletes a WHERE a.MeetEntryId = me.Id) = @Count
              AND (SELECT COUNT(*) FROM MeetEntryAthletes a WHERE a.MeetEntryId = me.Id AND a.AthleteId IN @Ids) = @Count
            ORDER BY me.Id", new { MeetId = meetId, w.EventId, Count = count, Ids = w.RelayAthleteIds }, tx);
    }

    private static async Task InsertEntryAsync(IDbConnection c, IDbTransaction tx, int meetId, MeetImportWrite w, int? performanceId)
    {
        var entryId = await c.ExecuteScalarAsync<int>(@"
            INSERT INTO MeetEntries (MeetId, EventId, AthleteId, PerformanceId)
            OUTPUT INSERTED.Id
            VALUES (@MeetId, @EventId, @AthleteId, @PerformanceId)",
            new { MeetId = meetId, w.EventId, AthleteId = w.IsRelay ? (int?)null : w.AthleteId, PerformanceId = performanceId }, tx);
        if (w.IsRelay)
        {
            await c.ExecuteAsync("INSERT INTO MeetEntryAthletes (MeetEntryId, AthleteId) VALUES (@MeetEntryId, @AthleteId)",
                w.RelayAthleteIds.Select(id => new { MeetEntryId = entryId, AthleteId = id }), tx);
        }
    }
}
