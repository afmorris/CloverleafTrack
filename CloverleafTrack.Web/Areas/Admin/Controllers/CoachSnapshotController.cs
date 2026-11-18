using CloverleafTrack.DataAccess.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CloverleafTrack.Areas.Admin.Controllers;

/// <summary>
/// GET /Admin/CoachSnapshot: the JSON snapshot that coach/scripts/push-snapshot.sh sends to coach.cloverleaftrack.com
/// each night. Read-only; the coaching subsite never writes back. BRAIN.md [C133].
/// </summary>
[Area("Admin")]
public class CoachSnapshotController(ICoachSnapshotRepository repository) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var s = await repository.GetSnapshotAsync();
        return Json(new
        {
            version = 1,
            generatedAt = DateTime.UtcNow.ToString("o"),
            athletes = s.Athletes,
            events = s.Events,
            seasons = s.Seasons.Select(x => new
            {
                x.Id, x.Name,
                StartDate = x.StartDate.ToString("yyyy-MM-dd"),
                EndDate = x.EndDate.ToString("yyyy-MM-dd"),
                x.IsCurrentSeason
            }),
            seasonBests = s.SeasonBests.Select(b => new
            {
                b.AthleteId, b.EventId, b.SeasonId, b.TimeSeconds, b.DistanceInches, b.MeetName,
                MeetDate = b.MeetDate.ToString("yyyy-MM-dd")
            }),
            meets = s.Meets.Select(m => new
            {
                m.Id, m.Name, Date = m.Date.ToString("yyyy-MM-dd"), m.Environment, m.SeasonId
            })
        });
    }
}
