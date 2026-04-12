using CloverleafTrack.DataAccess.Dtos;
using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using Dapper;

namespace CloverleafTrack.DataAccess.Repositories;

public class AdminMeetEntryRepository(IDbConnectionFactory connectionFactory) : IAdminMeetEntryRepository
{
    public async Task<List<MeetEntryDto>> GetForMeetAsync(int meetId)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            SELECT
                me.Id,
                me.MeetId,
                me.EventId,
                me.AthleteId,
                me.PerformanceId,
                e.Name       AS EventName,
                e.EventType,
                e.EventCategory,
                e.Gender     AS EventGender,
                e.AthleteCount AS EventAthleteCount,
                e.SortOrder  AS EventSortOrder,
                e.EventCategorySortOrder,
                a.FirstName  AS AthleteFirstName,
                a.LastName   AS AthleteLastName,
                a.Gender     AS AthleteGender,
                (
                    SELECT STRING_AGG(ra.FirstName + ' ' + ra.LastName, '|~|')
                    FROM MeetEntryAthletes mea2
                    INNER JOIN Athletes ra ON ra.Id = mea2.AthleteId
                    WHERE mea2.MeetEntryId = me.Id
                ) AS RelayAthleteNames,
                p.TimeSeconds,
                p.DistanceInches
            FROM MeetEntries me
            INNER JOIN Events e ON e.Id = me.EventId
            LEFT JOIN Athletes a ON a.Id = me.AthleteId
            LEFT JOIN Performances p ON p.Id = me.PerformanceId
            WHERE me.MeetId = @MeetId AND me.Deleted = 0
            ORDER BY e.EventCategorySortOrder, e.SortOrder, a.LastName, a.FirstName";

        var results = await connection.QueryAsync<MeetEntryDto>(sql, new { MeetId = meetId });
        return results.ToList();
    }

    public async Task<MeetEntryDto?> GetByIdAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            SELECT
                me.Id,
                me.MeetId,
                me.EventId,
                me.AthleteId,
                me.PerformanceId,
                e.Name       AS EventName,
                e.EventType,
                e.EventCategory,
                e.Gender     AS EventGender,
                e.AthleteCount AS EventAthleteCount,
                e.SortOrder  AS EventSortOrder,
                e.EventCategorySortOrder,
                a.FirstName  AS AthleteFirstName,
                a.LastName   AS AthleteLastName,
                a.Gender     AS AthleteGender,
                (
                    SELECT STRING_AGG(ra.FirstName + ' ' + ra.LastName, '|~|')
                    FROM MeetEntryAthletes mea2
                    INNER JOIN Athletes ra ON ra.Id = mea2.AthleteId
                    WHERE mea2.MeetEntryId = me.Id
                ) AS RelayAthleteNames,
                p.TimeSeconds,
                p.DistanceInches
            FROM MeetEntries me
            INNER JOIN Events e ON e.Id = me.EventId
            LEFT JOIN Athletes a ON a.Id = me.AthleteId
            LEFT JOIN Performances p ON p.Id = me.PerformanceId
            WHERE me.Id = @Id AND me.Deleted = 0";

        return await connection.QuerySingleOrDefaultAsync<MeetEntryDto>(sql, new { Id = id });
    }

    public async Task<int> CreateAsync(MeetEntry entry)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO MeetEntries (MeetId, EventId, AthleteId, PerformanceId)
            OUTPUT INSERTED.Id
            VALUES (@MeetId, @EventId, @AthleteId, @PerformanceId)";
        return await connection.ExecuteScalarAsync<int>(sql, entry);
    }

    public async Task<bool> UpdatePerformanceIdAsync(int entryId, int performanceId)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE MeetEntries
            SET PerformanceId = @PerformanceId
            WHERE Id = @EntryId AND Deleted = 0";
        var rowsAffected = await connection.ExecuteAsync(sql, new { EntryId = entryId, PerformanceId = performanceId });
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE MeetEntries
            SET Deleted = 1, DateDeleted = GETUTCDATE()
            WHERE Id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    public async Task AddRelayAthletesAsync(int meetEntryId, IEnumerable<int> athleteIds)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO MeetEntryAthletes (MeetEntryId, AthleteId)
            VALUES (@MeetEntryId, @AthleteId)";
        await connection.ExecuteAsync(sql,
            athleteIds.Select(id => new { MeetEntryId = meetEntryId, AthleteId = id }));
    }

    public async Task RemoveRelayAthletesAsync(int meetEntryId)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = "DELETE FROM MeetEntryAthletes WHERE MeetEntryId = @MeetEntryId";
        await connection.ExecuteAsync(sql, new { MeetEntryId = meetEntryId });
    }

    public async Task<List<int>> GetRelayAthleteIdsAsync(int meetEntryId)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = "SELECT AthleteId FROM MeetEntryAthletes WHERE MeetEntryId = @MeetEntryId";
        var ids = await connection.QueryAsync<int>(sql, new { MeetEntryId = meetEntryId });
        return ids.ToList();
    }

    public async Task<int> GetAthleteEventCountForMeetAsync(int meetId, int athleteId)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            SELECT COUNT(*) FROM (
                -- Individual entries
                SELECT me.Id
                FROM MeetEntries me
                WHERE me.MeetId = @MeetId
                  AND me.AthleteId = @AthleteId
                  AND me.Deleted = 0

                UNION ALL

                -- Relay entries where athlete is on the relay team
                SELECT me.Id
                FROM MeetEntries me
                INNER JOIN MeetEntryAthletes mea ON mea.MeetEntryId = me.Id
                WHERE me.MeetId = @MeetId
                  AND mea.AthleteId = @AthleteId
                  AND me.Deleted = 0
            ) AS combined";
        return await connection.ExecuteScalarAsync<int>(sql, new { MeetId = meetId, AthleteId = athleteId });
    }
}
