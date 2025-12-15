using CloverleafTrack.DataAccess.Dtos;
using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using Dapper;
using Slugify;

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

    public async Task<Meet?> GetMeetBasicInfoBySlugAsync(string slug)
     {
          using var connection = connectionFactory.CreateConnection();
          
          const string sql = """
                              SELECT
                                   m.*,
                                   l.*,
                                   s.*
                              FROM
                                   Meets m
                                   INNER JOIN Locations l ON m.LocationId = l.Id
                                   INNER JOIN Seasons s ON s.Id = m.SeasonId
                              """;
          
          var meets = await connection.QueryAsync<Meet, Location, Season, Meet>(
               sql,
               (meet, location, season) =>
               {
                    meet.Location = location;
                    meet.Season = season;
                    return meet;
               },
               splitOn: "Id,Id");
          
          // Filter by slug
          var slugHelper = new SlugHelper();
          return meets.FirstOrDefault(m => slugHelper.GenerateSlug(m.Name) == slug);
     }

     public async Task<List<MeetPerformanceDto>> GetPerformancesForMeetAsync(int meetId)
     {
          using var connection = connectionFactory.CreateConnection();
          
          const string sql = """
                              SELECT
                                   p.Id as PerformanceId,
                                   e.Id as EventId,
                                   e.Name as EventName,
                                   e.SortOrder as EventSortOrder,
                                   e.EventCategory,
                                   e.Gender as EventGender,
                                   a.Id as AthleteId,
                                   a.FirstName + ' ' + a.LastName as AthleteName,
                                   p.TimeSeconds,
                                   p.DistanceInches,
                                   p.PersonalBest,
                                   p.SchoolRecord,
                                   p.SeasonBest,
                                   lb.Rank as AllTimeRank
                              FROM
                                   Performances p
                                   INNER JOIN Events e ON e.Id = p.EventId
                                   LEFT JOIN Athletes a ON a.Id = p.AthleteId
                                   LEFT JOIN Leaderboards lb ON lb.PerformanceId = p.Id
                              WHERE
                                   p.MeetId = @MeetId
                              ORDER BY
                                   e.Gender,
                                   e.SortOrder,
                                   CASE WHEN p.TimeSeconds IS NOT NULL THEN p.TimeSeconds ELSE 999999 END ASC,
                                   p.DistanceInches DESC
                              """;
          
          var performances = await connection.QueryAsync<MeetPerformanceDto>(sql, new { MeetId = meetId });
          return performances.ToList();
     }
}