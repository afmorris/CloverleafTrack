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
            SELECT mp.*, s.*
            FROM MeetParticipants mp
            INNER JOIN Schools s ON s.Id = mp.SchoolId AND s.Deleted = 0
            WHERE mp.MeetId = @MeetId AND mp.Deleted = 0
            ORDER BY mp.SortOrder, s.Name";

        var results = await connection.QueryAsync<MeetParticipant, School, MeetParticipant>(
            sql,
            (participant, school) => { participant.School = school; return participant; },
            new { MeetId = meetId },
            splitOn: "Id");

        return results.ToList();
    }

    public async Task<MeetParticipant?> GetByIdAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            SELECT mp.*, s.*
            FROM MeetParticipants mp
            INNER JOIN Schools s ON s.Id = mp.SchoolId AND s.Deleted = 0
            WHERE mp.Id = @Id AND mp.Deleted = 0";

        var results = await connection.QueryAsync<MeetParticipant, School, MeetParticipant>(
            sql,
            (participant, school) => { participant.School = school; return participant; },
            new { Id = id },
            splitOn: "Id");

        return results.FirstOrDefault();
    }

    public async Task<int> CreateAsync(MeetParticipant participant)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO MeetParticipants (MeetId, SchoolId, Gender, SortOrder)
            OUTPUT INSERTED.Id
            VALUES (@MeetId, @SchoolId, @Gender, @SortOrder)";
        return await connection.ExecuteScalarAsync<int>(sql, participant);
    }

    public async Task<bool> UpdateAsync(MeetParticipant participant)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE MeetParticipants
            SET SchoolId = @SchoolId, Gender = @Gender, SortOrder = @SortOrder, DateUpdated = GETUTCDATE()
            WHERE Id = @Id AND Deleted = 0";
        var rowsAffected = await connection.ExecuteAsync(sql, participant);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = "UPDATE MeetParticipants SET Deleted = 1, DateDeleted = GETUTCDATE() WHERE Id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }
}
