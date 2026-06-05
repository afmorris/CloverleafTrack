using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using Dapper;

namespace CloverleafTrack.DataAccess.Repositories;

public class MeetPlacingRepository(IDbConnectionFactory connectionFactory) : IMeetPlacingRepository
{
    public async Task<List<MeetPlacing>> GetForMeetAsync(int meetId)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            SELECT mp.*, mpart.*, s.*
            FROM MeetPlacings mp
            LEFT JOIN MeetParticipants mpart ON mpart.Id = mp.MeetParticipantId AND mpart.Deleted = 0
            LEFT JOIN Schools s ON s.Id = mpart.SchoolId AND s.Deleted = 0
            WHERE mp.MeetId = @MeetId";

        var results = await connection.QueryAsync<MeetPlacing, MeetParticipant, School, MeetPlacing>(
            sql,
            (placing, participant, school) =>
            {
                if (participant != null)
                {
                    participant.School = school ?? new School();
                    placing.MeetParticipant = participant;
                }
                return placing;
            },
            new { MeetId = meetId },
            splitOn: "Id,Id");

        return results.ToList();
    }
}
