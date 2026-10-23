using CloverleafTrack.DataAccess.Dtos;

namespace CloverleafTrack.DataAccess.Interfaces;

public interface IAdminMeetImportRepository
{
    /// <summary>
    /// Applies every write in ONE transaction (all or nothing), then runs sp_RebuildLeaderboards once. The per-row repository
    /// methods rebuild after every insert (~3 s each), which is unusable for a whole meet.
    /// Results are linked to a matching unlinked pre-meet entry (same event and athlete(s)); when there is none an entry is created,
    /// so the meet's Entries page shows the result.
    /// </summary>
    Task<MeetImportOutcome> ApplyAsync(int meetId, IReadOnlyList<MeetImportWrite> writes);
}
