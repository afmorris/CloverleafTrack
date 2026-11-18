using CloverleafTrack.DataAccess.Dtos.Coach;
using CloverleafTrack.DataAccess.Interfaces;
using Dapper;

namespace CloverleafTrack.DataAccess.Repositories;

/// <summary>
/// One-way export for coach.cloverleaftrack.com: active athletes, individual events, and each athlete's best mark per
/// event per season (marks only, ResultStatus = 0). Relays are left out: the coaching tools plan individual work.
/// BRAIN.md [C133].
/// </summary>
public class CoachSnapshotRepository(IDbConnectionFactory connectionFactory) : ICoachSnapshotRepository
{
    private const string Sql = @"
        SELECT Id, FirstName, LastName, Gender, GraduationYear
        FROM Athletes WHERE IsActive = 1;

        SELECT Id, Name, EventKey, EventType, Gender, Environment, EventCategory, SortOrder
        FROM Events WHERE EventType IN (0, 1);

        SELECT Id, Name, StartDate, EndDate, IsCurrentSeason FROM Seasons;

        WITH ranked AS (
            SELECT p.AthleteId, p.EventId, m.SeasonId, p.TimeSeconds, p.DistanceInches,
                   m.Name AS MeetName, m.Date AS MeetDate,
                   ROW_NUMBER() OVER (
                       PARTITION BY p.AthleteId, p.EventId, m.SeasonId
                       ORDER BY CASE WHEN e.EventType = 1 THEN p.TimeSeconds ELSE -p.DistanceInches END, m.Date) AS rn
            FROM Performances p
            INNER JOIN Meets m ON m.Id = p.MeetId
            INNER JOIN Events e ON e.Id = p.EventId
            INNER JOIN Athletes a ON a.Id = p.AthleteId
            WHERE a.IsActive = 1 AND e.EventType IN (0, 1) AND p.ResultStatus = 0
              AND (p.TimeSeconds IS NOT NULL OR p.DistanceInches IS NOT NULL)
        )
        SELECT AthleteId, EventId, SeasonId, TimeSeconds, DistanceInches, MeetName, MeetDate
        FROM ranked WHERE rn = 1;

        SELECT m.Id, m.Name, m.Date, m.Environment, m.SeasonId
        FROM Meets m
        WHERE m.SeasonId IN (SELECT TOP 2 Id FROM Seasons ORDER BY StartDate DESC);";

    public async Task<CoachSnapshotDto> GetSnapshotAsync()
    {
        using var connection = connectionFactory.CreateConnection();
        using var grid = await connection.QueryMultipleAsync(Sql);
        return new CoachSnapshotDto
        {
            Athletes = (await grid.ReadAsync<CoachAthleteDto>()).ToList(),
            Events = (await grid.ReadAsync<CoachEventDto>()).ToList(),
            Seasons = (await grid.ReadAsync<CoachSeasonDto>()).ToList(),
            SeasonBests = (await grid.ReadAsync<CoachSeasonBestDto>()).ToList(),
            Meets = (await grid.ReadAsync<CoachMeetDto>()).ToList()
        };
    }
}
