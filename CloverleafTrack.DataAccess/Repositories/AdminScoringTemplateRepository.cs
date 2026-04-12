using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using Dapper;

namespace CloverleafTrack.DataAccess.Repositories;

public class AdminScoringTemplateRepository(IDbConnectionFactory connectionFactory) : IAdminScoringTemplateRepository
{
    public async Task<List<ScoringTemplate>> GetAllAsync()
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            SELECT st.*, stp.*
            FROM ScoringTemplates st
            LEFT JOIN ScoringTemplatePlaces stp ON stp.ScoringTemplateId = st.Id
            WHERE st.Deleted = 0
            ORDER BY st.IsBuiltIn DESC, st.Name, stp.Place";

        var templateDict = new Dictionary<int, ScoringTemplate>();

        await connection.QueryAsync<ScoringTemplate, ScoringTemplatePlace, ScoringTemplate>(
            sql,
            (template, place) =>
            {
                if (!templateDict.TryGetValue(template.Id, out var existing))
                {
                    existing = template;
                    templateDict[template.Id] = existing;
                }
                if (place != null)
                    existing.Places.Add(place);
                return existing;
            },
            splitOn: "Id");

        return templateDict.Values.ToList();
    }

    public async Task<ScoringTemplate?> GetByIdAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            SELECT st.*, stp.*
            FROM ScoringTemplates st
            LEFT JOIN ScoringTemplatePlaces stp ON stp.ScoringTemplateId = st.Id
            WHERE st.Id = @Id AND st.Deleted = 0
            ORDER BY stp.Place";

        ScoringTemplate? result = null;

        await connection.QueryAsync<ScoringTemplate, ScoringTemplatePlace, ScoringTemplate>(
            sql,
            (template, place) =>
            {
                result ??= template;
                if (place != null)
                    result.Places.Add(place);
                return result;
            },
            new { Id = id },
            splitOn: "Id");

        return result;
    }

    public async Task<int> CreateAsync(ScoringTemplate template)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO ScoringTemplates (Name, IsBuiltIn)
            OUTPUT INSERTED.Id
            VALUES (@Name, @IsBuiltIn)";
        return await connection.ExecuteScalarAsync<int>(sql, template);
    }

    public async Task<bool> UpdateAsync(ScoringTemplate template)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE ScoringTemplates
            SET Name = @Name,
                DateUpdated = GETUTCDATE()
            WHERE Id = @Id AND Deleted = 0";
        var rowsAffected = await connection.ExecuteAsync(sql, template);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE ScoringTemplates
            SET Deleted = 1, DateDeleted = GETUTCDATE()
            WHERE Id = @Id AND IsBuiltIn = 0";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    public async Task<int> AddPlaceAsync(ScoringTemplatePlace place)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO ScoringTemplatePlaces (ScoringTemplateId, Place, Points)
            OUTPUT INSERTED.Id
            VALUES (@ScoringTemplateId, @Place, @Points)";
        return await connection.ExecuteScalarAsync<int>(sql, place);
    }

    public async Task<bool> UpdatePlaceAsync(ScoringTemplatePlace place)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE ScoringTemplatePlaces
            SET Points = @Points,
                DateUpdated = GETUTCDATE()
            WHERE Id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, place);
        return rowsAffected > 0;
    }

    public async Task<bool> DeletePlaceAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = "DELETE FROM ScoringTemplatePlaces WHERE Id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAllPlacesAsync(int templateId)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = "DELETE FROM ScoringTemplatePlaces WHERE ScoringTemplateId = @TemplateId";
        await connection.ExecuteAsync(sql, new { TemplateId = templateId });
        return true;
    }
}
