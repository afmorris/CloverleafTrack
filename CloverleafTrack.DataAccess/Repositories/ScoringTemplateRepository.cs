using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using Dapper;

namespace CloverleafTrack.DataAccess.Repositories;

public class ScoringTemplateRepository(IDbConnectionFactory connectionFactory) : IScoringTemplateRepository
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
}
