using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using CloverleafTrack.Models.Enums;
using Dapper;

namespace CloverleafTrack.DataAccess.Repositories;

public class AthleteSummaryRepository(IDbConnectionFactory connectionFactory) : IAthleteSummaryRepository
{
    public async Task<AthleteSummary?> GetForAthleteAsync(int athleteId)
    {
        using var connection = connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<AthleteSummary>(
            "SELECT * FROM AthleteSummaries WHERE AthleteId = @AthleteId", new { AthleteId = athleteId });
    }

    public async Task<List<AthleteSummary>> GetForAthletesAsync(IEnumerable<int> athleteIds)
    {
        var ids = athleteIds.ToList();
        if (ids.Count == 0) return new List<AthleteSummary>();

        using var connection = connectionFactory.CreateConnection();
        var rows = await connection.QueryAsync<AthleteSummary>(
            "SELECT * FROM AthleteSummaries WHERE AthleteId IN @Ids", new { Ids = ids });
        return rows.ToList();
    }

    public async Task SaveDraftAsync(int athleteId, string text, string inputHash, string model)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE AthleteSummaries
            SET SummaryText = @Text, InputHash = @InputHash, Model = @Model, Status = @Draft,
                GeneratedAt = GETUTCDATE(), ReviewedAt = NULL
            WHERE AthleteId = @AthleteId;
            IF @@ROWCOUNT = 0
                INSERT INTO AthleteSummaries (AthleteId, SummaryText, InputHash, Model, Status)
                VALUES (@AthleteId, @Text, @InputHash, @Model, @Draft);";
        await connection.ExecuteAsync(sql, new
        {
            AthleteId = athleteId, Text = text, InputHash = inputHash, Model = model, Draft = (short)SummaryStatus.Draft
        });
    }

    public async Task UpdateTextAsync(int id, string text)
    {
        using var connection = connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            "UPDATE AthleteSummaries SET SummaryText = @Text, Status = @Draft, ReviewedAt = NULL WHERE Id = @Id",
            new { Id = id, Text = text, Draft = (short)SummaryStatus.Draft });
    }

    public async Task SetApprovedAsync(int id, bool approved)
    {
        using var connection = connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            "UPDATE AthleteSummaries SET Status = @Status, ReviewedAt = @ReviewedAt WHERE Id = @Id",
            new
            {
                Id = id,
                Status = (short)(approved ? SummaryStatus.Approved : SummaryStatus.Draft),
                ReviewedAt = approved ? DateTime.UtcNow : (DateTime?)null
            });
    }

    public async Task DeleteAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        await connection.ExecuteAsync("DELETE FROM AthleteSummaries WHERE Id = @Id", new { Id = id });
    }
}
