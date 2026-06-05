using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using Dapper;

namespace CloverleafTrack.DataAccess.Repositories;

public class AdminMeetTeamResultRepository(IDbConnectionFactory connectionFactory) : IAdminMeetTeamResultRepository
{
    public async Task<List<MeetTeamResult>> GetForMeetAsync(int meetId)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            SELECT mtr.*, mp.*, s.*
            FROM MeetTeamResults mtr
            LEFT JOIN MeetParticipants mp ON mp.Id = mtr.OpponentMeetParticipantId AND mp.Deleted = 0
            LEFT JOIN Schools s ON s.Id = mp.SchoolId AND s.Deleted = 0
            WHERE mtr.MeetId = @MeetId AND mtr.Deleted = 0
            ORDER BY mtr.Gender, mtr.Id";

        var results = await connection.QueryAsync<MeetTeamResult, MeetParticipant, School, MeetTeamResult>(
            sql,
            (result, participant, school) =>
            {
                if (participant != null)
                {
                    participant.School = school ?? new School();
                    result.OpponentMeetParticipant = participant;
                }
                return result;
            },
            new { MeetId = meetId },
            splitOn: "Id,Id");

        return results.ToList();
    }

    public async Task<int> CreateAsync(MeetTeamResult result)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO MeetTeamResults (MeetId, Gender, OpponentMeetParticipantId, OurScore, OpponentScore, Place, FieldSize)
            OUTPUT INSERTED.Id
            VALUES (@MeetId, @Gender, @OpponentMeetParticipantId, @OurScore, @OpponentScore, @Place, @FieldSize)";
        return await connection.ExecuteScalarAsync<int>(sql, result);
    }

    public async Task<bool> UpdateAsync(MeetTeamResult result)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE MeetTeamResults
            SET OurScore = @OurScore, OpponentScore = @OpponentScore,
                Place = @Place, FieldSize = @FieldSize, DateUpdated = GETUTCDATE()
            WHERE Id = @Id AND Deleted = 0";
        var rows = await connection.ExecuteAsync(sql, result);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = "UPDATE MeetTeamResults SET Deleted = 1, DateDeleted = GETUTCDATE() WHERE Id = @Id";
        var rows = await connection.ExecuteAsync(sql, new { Id = id });
        return rows > 0;
    }

    public async Task DeleteAllForMeetAsync(int meetId)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = "UPDATE MeetTeamResults SET Deleted = 1, DateDeleted = GETUTCDATE() WHERE MeetId = @MeetId AND Deleted = 0";
        await connection.ExecuteAsync(sql, new { MeetId = meetId });
    }
}
