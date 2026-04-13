using CloverleafTrack.DataAccess.Dtos;
using CloverleafTrack.DataAccess.Interfaces;
using Dapper;

namespace CloverleafTrack.DataAccess.Repositories;

public class SeasonScoringRepository(IDbConnectionFactory connectionFactory) : ISeasonScoringRepository
{
    public async Task<List<ScoringDataDto>> GetScoringDataForSeasonAsync(int seasonId)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            -- Individual performances with placings
            SELECT
                mp.Id           AS PlacingId,
                m.Id            AS MeetId,
                m.Name          AS MeetName,
                m.Date          AS MeetDate,
                p.Id            AS PerformanceId,
                e.Id            AS EventId,
                e.Name          AS EventName,
                e.EventType,
                e.EventCategory,
                e.AthleteCount  AS EventAthleteCount,
                a.Id            AS AthleteId,
                a.FirstName     AS AthleteFirstName,
                a.LastName      AS AthleteLastName,
                a.Gender        AS AthleteGender,
                mp.MeetParticipantId,
                mpart.SchoolName AS OpponentSchoolName,
                mp.Place,
                mp.FullPoints,
                mp.SplitPoints
            FROM MeetPlacings mp
            INNER JOIN Performances p     ON p.Id  = mp.PerformanceId
            INNER JOIN Events e           ON e.Id  = p.EventId
            INNER JOIN Meets m            ON m.Id  = p.MeetId
            INNER JOIN Seasons s          ON s.Id  = m.SeasonId
            INNER JOIN Athletes a         ON a.Id  = p.AthleteId
            LEFT  JOIN MeetParticipants mpart ON mpart.Id = mp.MeetParticipantId
            WHERE s.Id = @SeasonId
              AND p.AthleteId IS NOT NULL

            UNION ALL

            -- Relay performances expanded per relay member
            SELECT
                mp.Id           AS PlacingId,
                m.Id            AS MeetId,
                m.Name          AS MeetName,
                m.Date          AS MeetDate,
                p.Id            AS PerformanceId,
                e.Id            AS EventId,
                e.Name          AS EventName,
                e.EventType,
                e.EventCategory,
                e.AthleteCount  AS EventAthleteCount,
                a.Id            AS AthleteId,
                a.FirstName     AS AthleteFirstName,
                a.LastName      AS AthleteLastName,
                a.Gender        AS AthleteGender,
                mp.MeetParticipantId,
                mpart.SchoolName AS OpponentSchoolName,
                mp.Place,
                mp.FullPoints,
                mp.SplitPoints
            FROM MeetPlacings mp
            INNER JOIN Performances p          ON p.Id  = mp.PerformanceId
            INNER JOIN Events e                ON e.Id  = p.EventId
            INNER JOIN Meets m                 ON m.Id  = p.MeetId
            INNER JOIN Seasons s               ON s.Id  = m.SeasonId
            INNER JOIN PerformanceAthletes pa  ON pa.PerformanceId = p.Id
            INNER JOIN Athletes a              ON a.Id  = pa.AthleteId
            LEFT  JOIN MeetParticipants mpart  ON mpart.Id = mp.MeetParticipantId
            WHERE s.Id = @SeasonId
              AND p.AthleteId IS NULL

            ORDER BY MeetDate, EventId, AthleteLastName, AthleteFirstName";

        var results = await connection.QueryAsync<ScoringDataDto>(sql, new { SeasonId = seasonId });
        return results.ToList();
    }
}
