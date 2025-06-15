using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using Dapper;

namespace CloverleafTrack.DataAccess.Repositories;

public class SeasonRepository(IDbConnectionFactory connectionFactory) : ISeasonRepository
{
    public async Task<List<Season>> GetAllAsync()
    {
        using var connection = connectionFactory.CreateConnection();
        var sql = "SELECT * FROM Seasons";
        var result = await connection.QueryAsync<Season>(sql);
        return result.ToList();
    }

    public async Task<List<Season>> GetSeasonsWithMeetsAsync()
    {
        using var connection = connectionFactory.CreateConnection();

        var sql = @"
            SELECT 
                s.*, 
                m.*, 
                p.*, 
                e.*,
                a.*
            FROM Seasons s
            LEFT JOIN Meets m ON m.SeasonId = s.Id
            LEFT JOIN Performances p ON p.MeetId = m.Id
            LEFT JOIN Events e ON e.Id = p.EventId
            LEFT JOIN Athletes a ON a.Id = p.AthleteId
            ORDER BY s.StartDate DESC, m.Date";

        var seasonMap = new Dictionary<int, Season>();
        var meetMap = new Dictionary<int, Meet>();

        var result = await connection.QueryAsync<Season, Meet, Performance, Event, Athlete, Season>(
            sql,
            (season, meet, performance, evt, athlete) =>
            {
                if (!seasonMap.TryGetValue(season.Id, out var currentSeason))
                {
                    currentSeason = season;
                    currentSeason.Meets = new List<Meet>();
                    seasonMap[season.Id] = currentSeason;
                }

                if (meet != null)
                {
                    if (!meetMap.TryGetValue(meet.Id, out var currentMeet))
                    {
                        currentMeet = meet;
                        currentMeet.Performances = new List<Performance>();
                        currentSeason.Meets.Add(currentMeet);
                        meetMap[meet.Id] = currentMeet;
                    }
                    
                    performance.Event = evt;
                    performance.Athlete = athlete;
                    meetMap[meet.Id].Performances.Add(performance);
                }

                return currentSeason;
            },
            splitOn: "Id,Id,Id,Id");

        return seasonMap.Values.ToList();
    }

    public async Task<Season?> GetByIdAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        var sql = "SELECT * FROM Seasons WHERE Id = @Id";
        return await connection.QueryFirstOrDefaultAsync<Season>(sql, new { Id = id });
    }

    public async Task<int> CreateAsync(Season season)
    {
        using var connection = connectionFactory.CreateConnection();
        var sql = @"
            INSERT INTO Seasons (Name, StartDate, EndDate)
            VALUES (@Name, @StartDate, @EndDate);
            SELECT CAST(SCOPE_IDENTITY() as int)";
        return await connection.ExecuteScalarAsync<int>(sql, season);
    }

    public async Task<bool> UpdateAsync(Season season)
    {
        using var connection = connectionFactory.CreateConnection();
        var sql = @"
            UPDATE Seasons
            SET Name = @Name,
                StartDate = @StartDate,
                EndDate = @EndDate
            WHERE Id = @Id";
        var affectedRows = await connection.ExecuteAsync(sql, season);
        return affectedRows > 0;
    }

    public async Task<bool> DeleteAsync(Season season)
    {
        using var connection = connectionFactory.CreateConnection();
        var sql = "DELETE FROM Seasons WHERE Id = @Id";
        var affectedRows = await connection.ExecuteAsync(sql, new { season.Id });
        return affectedRows > 0;
    }
}