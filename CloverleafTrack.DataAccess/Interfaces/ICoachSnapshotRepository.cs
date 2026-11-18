using CloverleafTrack.DataAccess.Dtos.Coach;

namespace CloverleafTrack.DataAccess.Interfaces;

/// <summary>Read-only data for the coaching subsite's nightly snapshot. BRAIN.md [C133].</summary>
public interface ICoachSnapshotRepository
{
    Task<CoachSnapshotDto> GetSnapshotAsync();
}
