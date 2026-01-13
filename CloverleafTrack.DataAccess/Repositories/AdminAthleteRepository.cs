using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using Dapper;

namespace CloverleafTrack.DataAccess.Repositories;

public class AdminAthleteRepository(IDbConnectionFactory connectionFactory) : IAdminAthleteRepository
{
    public async Task<List<Athlete>> GetAllAthletesAsync()
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Athletes ORDER BY LastName, FirstName";
        var athletes = await connection.QueryAsync<Athlete>(sql);
        return athletes.ToList();
    }

    public async Task<List<Athlete>> GetAthletesByFiltersAsync(string? searchTerm, short? gender, bool? isActive, int? graduationYear)
    {
        using var connection = connectionFactory.CreateConnection();

        var sql = @"
            SELECT * FROM Athletes 
            WHERE 1=1
                AND (@SearchTerm IS NULL OR FirstName LIKE '%' + @SearchTerm + '%' OR LastName LIKE '%' + @SearchTerm + '%')
                AND (@Gender IS NULL OR Gender = @Gender)
                AND (@IsActive IS NULL OR IsActive = @IsActive)
                AND (@GraduationYear IS NULL OR GraduationYear = @GraduationYear)
            ORDER BY LastName, FirstName";

        var athletes = await connection.QueryAsync<Athlete>(sql, new { SearchTerm = searchTerm, Gender = gender, IsActive = isActive, GraduationYear = graduationYear });
        return athletes.ToList();
    }

    public async Task<List<Athlete>> GetAthletesEligibleForMeetAsync(DateTime meetDate, short? eventGender)
    {
        using var connection = connectionFactory.CreateConnection();

        // Athletes are eligible if they graduated 0-3 years after the meet year
        var meetYear = meetDate.Year;
        var minGradYear = meetYear;
        var maxGradYear = meetYear + 3;

        var sql = @"
            SELECT * FROM Athletes 
            WHERE GraduationYear BETWEEN @MinGradYear AND @MaxGradYear
                AND (@EventGender IS NULL OR Gender = @EventGender)
            ORDER BY LastName, FirstName";

        var athletes = await connection.QueryAsync<Athlete>(sql, new { MinGradYear = minGradYear, MaxGradYear = maxGradYear, EventGender = eventGender });
        return athletes.ToList();
    }

    public async Task<Athlete?> GetAthleteByIdAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Athletes WHERE Id = @Id";
        return await connection.QuerySingleOrDefaultAsync<Athlete>(sql, new { Id = id });
    }

    public async Task<int> CreateAthleteAsync(Athlete athlete)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO Athletes (FirstName, LastName, Gender, GraduationYear, IsActive)
            OUTPUT INSERTED.Id
            VALUES (@FirstName, @LastName, @Gender, @GraduationYear, @IsActive)";
        return await connection.ExecuteScalarAsync<int>(sql, athlete);
    }

    public async Task<bool> UpdateAthleteAsync(Athlete athlete)
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

    public async Task<bool> DeleteAthleteAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = "DELETE FROM Athletes WHERE Id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    public async Task<List<Athlete>> FindSimilarAthletesAsync(string firstName, string lastName)
    {
        using var connection = connectionFactory.CreateConnection();

        const string sql = @"
            SELECT * FROM Athletes 
            WHERE (FirstName LIKE '%' + @FirstName + '%' OR @FirstName LIKE '%' + FirstName + '%')
                AND (LastName LIKE '%' + @LastName + '%' OR @LastName LIKE '%' + LastName + '%')
            ORDER BY LastName, FirstName";

        var athletes = await connection.QueryAsync<Athlete>(sql, new { FirstName = firstName, LastName = lastName });
        return athletes.ToList();
    }

    public async Task<int> GetPerformanceCountForAthleteAsync(int athleteId)
    {
        using var connection = connectionFactory.CreateConnection();

        const string sql = @"
            SELECT COUNT(*) 
            FROM Performances 
            WHERE AthleteId = @AthleteId";

        return await connection.ExecuteScalarAsync<int>(sql, new { AthleteId = athleteId });
    }
}