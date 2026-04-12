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
}
