using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using CloverleafTrack.Models.Enums;
using Dapper;

namespace CloverleafTrack.DataAccess.Repositories;

public class AdminAthleteRepository(IDbConnectionFactory connectionFactory) : IAdminAthleteRepository
{
    public async Task<List<Athlete>> GetAllAsync()
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Athletes ORDER BY LastName, FirstName";
        var athletes = await connection.QueryAsync<Athlete>(sql);
        return athletes.ToList();
    }

    public async Task<List<Athlete>> GetFilteredAsync(string? searchName, Gender? gender, bool? isActive, int? graduationYear)
    {
        using var connection = connectionFactory.CreateConnection();
        
        var sql = "SELECT * FROM Athletes WHERE 1=1";
        var parameters = new DynamicParameters();
        
        if (!string.IsNullOrWhiteSpace(searchName))
        {
            sql += " AND (FirstName LIKE @SearchName OR LastName LIKE @SearchName)";
            parameters.Add("SearchName", $"%{searchName}%");
        }
        
        if (gender.HasValue)
        {
            sql += " AND Gender = @Gender";
            parameters.Add("Gender", gender.Value);
        }
        
        if (isActive.HasValue)
        {
            sql += " AND IsActive = @IsActive";
            parameters.Add("IsActive", isActive.Value);
        }
        
        if (graduationYear.HasValue)
        {
            sql += " AND GraduationYear = @GraduationYear";
            parameters.Add("GraduationYear", graduationYear.Value);
        }
        
        sql += " ORDER BY LastName, FirstName";
        
        var athletes = await connection.QueryAsync<Athlete>(sql, parameters);
        return athletes.ToList();
    }

    public async Task<Athlete?> GetByIdAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Athletes WHERE Id = @Id";
        return await connection.QuerySingleOrDefaultAsync<Athlete>(sql, new { Id = id });
    }

    public async Task<int> CreateAsync(Athlete athlete)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO Athletes (FirstName, LastName, Gender, GraduationYear, IsActive)
            OUTPUT INSERTED.Id
            VALUES (@FirstName, @LastName, @Gender, @GraduationYear, @IsActive)";
        return await connection.ExecuteScalarAsync<int>(sql, athlete);
    }

    public async Task<bool> UpdateAsync(Athlete athlete)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE Athletes
            SET FirstName = @FirstName,
                LastName = @LastName,
                Gender = @Gender,
                GraduationYear = @GraduationYear,
                IsActive = @IsActive
            WHERE Id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, athlete);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = "DELETE FROM Athletes WHERE Id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    public async Task<List<Athlete>> GetSimilarAthletesAsync(string firstName, string lastName)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            SELECT * FROM Athletes
            WHERE (FirstName LIKE @FirstName OR LastName LIKE @LastName)
            ORDER BY LastName, FirstName";
        
        var athletes = await connection.QueryAsync<Athlete>(sql, new 
        { 
            FirstName = $"%{firstName}%", 
            LastName = $"%{lastName}%" 
        });
        return athletes.ToList();
    }

    public async Task<int> GetPerformanceCountAsync(int athleteId)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            SELECT COUNT(*)
            FROM Performances
            WHERE AthleteId = @AthleteId";
        return await connection.ExecuteScalarAsync<int>(sql, new { AthleteId = athleteId });
    }

    public async Task<List<Athlete>> GetAthletesForMeetAsync(int meetId, Gender? gender)
    {
        using var connection = connectionFactory.CreateConnection();
        
        // Get meet date to filter by graduation year
        const string meetSql = "SELECT Date FROM Meets WHERE Id = @MeetId";
        var meetDate = await connection.QuerySingleAsync<DateTime>(meetSql, new { MeetId = meetId });
        
        var meetYear = meetDate.Year;
        var minGradYear = meetYear - 3; // Athletes who graduated up to 3 years before the meet
        var maxGradYear = meetYear + 4; // Athletes who will graduate up to 4 years after the meet
        
        // Remove IsActive filter - show ALL athletes eligible based on graduation year
        var sql = @"
            SELECT * FROM Athletes
            WHERE GraduationYear BETWEEN @MinGradYear AND @MaxGradYear";
        
        var parameters = new DynamicParameters();
        parameters.Add("MinGradYear", minGradYear);
        parameters.Add("MaxGradYear", maxGradYear);
        
        if (gender.HasValue)
        {
            sql += " AND Gender = @Gender";
            parameters.Add("Gender", gender.Value);
        }
        
        sql += " ORDER BY LastName, FirstName";
        
        var athletes = await connection.QueryAsync<Athlete>(sql, parameters);
        return athletes.ToList();
    }
}