using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using Dapper;

namespace CloverleafTrack.DataAccess.Repositories;

public class AdminMeetParticipantRepository(IDbConnectionFactory connectionFactory) : IAdminMeetParticipantRepository
{
    public async Task<List<MeetParticipant>> GetForMeetAsync(int meetId)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            SELECT * FROM MeetParticipants
            WHERE MeetId = @MeetId AND Deleted = 0
            ORDER BY SortOrder, SchoolName";
        var results = await connection.QueryAsync<MeetParticipant>(sql, new { MeetId = meetId });
        return results.ToList();
    }

    public async Task<MeetParticipant?> GetByIdAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM MeetParticipants WHERE Id = @Id AND Deleted = 0";
        return await connection.QuerySingleOrDefaultAsync<MeetParticipant>(sql, new { Id = id });
    }

    public async Task<int> CreateAsync(MeetParticipant participant)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO MeetParticipants (MeetId, SchoolName, SortOrder)
            OUTPUT INSERTED.Id
            VALUES (@MeetId, @SchoolName, @SortOrder)";
        return await connection.ExecuteScalarAsync<int>(sql, participant);
    }

    public async Task<bool> UpdateAsync(MeetParticipant participant)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE MeetParticipants
            SET SchoolName = @SchoolName,
                SortOrder = @SortOrder,
                DateUpdated = GETUTCDATE()
            WHERE Id = @Id AND Deleted = 0";
        var rowsAffected = await connection.ExecuteAsync(sql, participant);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE MeetParticipants
            SET Deleted = 1, DateDeleted = GETUTCDATE()
            WHERE Id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }
}
