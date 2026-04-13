using CloverleafTrack.Models;

namespace CloverleafTrack.DataAccess.Interfaces;

public interface IAdminMeetPlacingRepository
{
    Task<List<MeetPlacing>> GetForMeetAsync(int meetId);
    Task<MeetPlacing?> GetByIdAsync(int id);
    Task<MeetPlacing?> GetByPerformanceAndParticipantAsync(int performanceId, int? meetParticipantId);
    Task<int> UpsertAsync(MeetPlacing placing);
    Task<bool> DeleteAsync(int id);
    Task<bool> DeleteByPerformanceAsync(int performanceId);

    /// <summary>
    /// Returns the points for a given place from the effective scoring template for this
    /// event in this meet (event-level override wins; meet default is the fallback).
    /// Returns 0 if the place is beyond the template's range.
    /// </summary>
    Task<decimal> GetTemplatePointsAsync(int meetId, int eventId, int place);
}
