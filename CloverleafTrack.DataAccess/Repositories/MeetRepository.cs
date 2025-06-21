using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using Dapper;

namespace CloverleafTrack.DataAccess.Repositories;

public class MeetRepository(IDbConnectionFactory connectionFactory) : IMeetRepository
{
    public async Task<List<Meet>> GetMeetsForSeasonAsync(int seasonId)
    {
        using  var connection = connectionFactory.CreateConnection();
        const string sql = """
                           SELECT
                                m.*,
                                COUNT(CASE WHEN p.PersonalBest = 1 THEN 1 END) AS PRCount,
                                COUNT(CASE WHEN p.SchoolRecord = 1 THEN 1 END) AS SchoolRecordCount,
                                l.*
                           FROM
                                Meets m
                                INNER JOIN Locations l ON m.LocationId = l.Id
                                LEFT JOIN Performances p ON p.MeetId = m.Id
                           WHERE
                                m.SeasonId = @SeasonId
                           GROUP BY
                                m.Id,
                                m.Name,
                                m.Date,
                                m.Environment,
                                m.HandTimed,
                                m.LocationId,
                                m.SeasonId,
                                m.EntryStatus,
                                m.EntryNotes,
                                l.Id,
                                l.Name,
                                l.City,
                                l.State,
                                l.ZipCode,
                                l.Country
                           ORDER BY
                                m.Date
                           """;

        var result = await connection.QueryAsync<Meet, Location, Meet>(
            sql,
            (meet, location) =>
            {
                meet.Location = location;
                return meet;
            },
            new { SeasonId = seasonId },
            splitOn: "Id,Id");
        
        return result.ToList();
    }
}