using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using Dapper;

namespace CloverleafTrack.DataAccess.Repositories;

public class AdminMeetPlacingRepository(IDbConnectionFactory connectionFactory) : IAdminMeetPlacingRepository
{
    public async Task<List<MeetPlacing>> GetForMeetAsync(int meetId)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            SELECT mp.*, mpart.*
            FROM MeetPlacings mp
            LEFT JOIN MeetParticipants mpart ON mpart.Id = mp.MeetParticipantId AND mpart.Deleted = 0
            WHERE mp.MeetId = @MeetId";

        var results = await connection.QueryAsync<MeetPlacing, MeetParticipant, MeetPlacing>(
            sql,
            (placing, participant) =>
            {
                placing.MeetParticipant = participant;
                return placing;
            },
            new { MeetId = meetId },
            splitOn: "Id");

        return results.ToList();
    }

    public async Task<MeetPlacing?> GetByIdAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            SELECT mp.*, mpart.*
            FROM MeetPlacings mp
            LEFT JOIN MeetParticipants mpart ON mpart.Id = mp.MeetParticipantId AND mpart.Deleted = 0
            WHERE mp.Id = @Id";

        var results = await connection.QueryAsync<MeetPlacing, MeetParticipant, MeetPlacing>(
            sql,
            (placing, participant) =>
            {
                placing.MeetParticipant = participant;
                return placing;
            },
            new { Id = id },
            splitOn: "Id");

        return results.FirstOrDefault();
    }

    public async Task<MeetPlacing?> GetByPerformanceAndParticipantAsync(int performanceId, int? meetParticipantId)
    {
        using var connection = connectionFactory.CreateConnection();

        if (meetParticipantId.HasValue)
        {
            const string sql = @"
                SELECT * FROM MeetPlacings
                WHERE PerformanceId = @PerformanceId
                  AND MeetParticipantId = @MeetParticipantId";
            return await connection.QuerySingleOrDefaultAsync<MeetPlacing>(sql,
                new { PerformanceId = performanceId, MeetParticipantId = meetParticipantId.Value });
        }
        else
        {
            const string sql = @"
                SELECT * FROM MeetPlacings
                WHERE PerformanceId = @PerformanceId
                  AND MeetParticipantId IS NULL";
            return await connection.QuerySingleOrDefaultAsync<MeetPlacing>(sql,
                new { PerformanceId = performanceId });
        }
    }

    public async Task<int> UpsertAsync(MeetPlacing placing)
    {
        using var connection = connectionFactory.CreateConnection();

        var existing = await GetByPerformanceAndParticipantAsync(placing.PerformanceId, placing.MeetParticipantId);

        if (existing != null)
        {
            const string updateSql = @"
                UPDATE MeetPlacings
                SET Place = @Place,
                    FullPoints = @FullPoints,
                    SplitPoints = @SplitPoints
                WHERE Id = @Id";
            placing.Id = existing.Id;
            await connection.ExecuteAsync(updateSql, placing);
            return existing.Id;
        }

        const string insertSql = @"
            INSERT INTO MeetPlacings (MeetId, PerformanceId, MeetParticipantId, Place, FullPoints, SplitPoints)
            OUTPUT INSERTED.Id
            VALUES (@MeetId, @PerformanceId, @MeetParticipantId, @Place, @FullPoints, @SplitPoints)";
        return await connection.ExecuteScalarAsync<int>(insertSql, placing);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = "DELETE FROM MeetPlacings WHERE Id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteByPerformanceAsync(int performanceId)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = "DELETE FROM MeetPlacings WHERE PerformanceId = @PerformanceId";
        await connection.ExecuteAsync(sql, new { PerformanceId = performanceId });
        return true;
    }

    public async Task<decimal> GetTemplatePointsAsync(int meetId, int eventId, int place)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            SELECT ISNULL(
                (SELECT stp.Points
                 FROM ScoringTemplatePlaces stp
                 WHERE stp.ScoringTemplateId = COALESCE(
                     (SELECT eo.ScoringTemplateId
                      FROM MeetEventScoringOverrides eo
                      WHERE eo.MeetId = @MeetId AND eo.EventId = @EventId),
                     (SELECT m.ScoringTemplateId FROM Meets m WHERE m.Id = @MeetId)
                 )
                 AND stp.Place = @Place),
            0)";
        return await connection.ExecuteScalarAsync<decimal>(sql,
            new { MeetId = meetId, EventId = eventId, Place = place });
    }
}
