using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using CloverleafTrack.Models.Enums;
using Dapper;

namespace CloverleafTrack.DataAccess.Repositories;

public class SeasonSummaryRepository(IDbConnectionFactory connectionFactory) : ISeasonSummaryRepository
{
    public async Task<SeasonSummary?> GetForSeasonAsync(int seasonId)
    {
        using var connection = connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<SeasonSummary>(
            "SELECT * FROM SeasonSummaries WHERE SeasonId = @SeasonId", new { SeasonId = seasonId });
    }

    public async Task<List<SeasonSummary>> GetAllAsync()
    {
        using var connection = connectionFactory.CreateConnection();
        return (await connection.QueryAsync<SeasonSummary>("SELECT * FROM SeasonSummaries")).ToList();
    }

    public async Task<string?> GetApprovedTextAsync(int seasonId)
    {
        using var connection = connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<string?>(
            "SELECT SummaryText FROM SeasonSummaries WHERE SeasonId = @SeasonId AND Status = @Approved",
            new { SeasonId = seasonId, Approved = (short)SummaryStatus.Approved });
    }

    public async Task SaveDraftAsync(int seasonId, string text, string inputHash, string model)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE SeasonSummaries
            SET SummaryText = @Text, InputHash = @InputHash, Model = @Model, Status = @Draft,
                GeneratedAt = GETUTCDATE(), ReviewedAt = NULL
            WHERE SeasonId = @SeasonId;
            IF @@ROWCOUNT = 0
                INSERT INTO SeasonSummaries (SeasonId, SummaryText, InputHash, Model, Status)
                VALUES (@SeasonId, @Text, @InputHash, @Model, @Draft);";
        await connection.ExecuteAsync(sql, new
        {
            SeasonId = seasonId, Text = text, InputHash = inputHash, Model = model, Draft = (short)SummaryStatus.Draft
        });
    }

    public async Task UpdateTextAsync(int id, string text)
    {
        using var connection = connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            "UPDATE SeasonSummaries SET SummaryText = @Text, Status = @Draft, ReviewedAt = NULL WHERE Id = @Id",
            new { Id = id, Text = text, Draft = (short)SummaryStatus.Draft });
    }

    public async Task SetApprovedAsync(int id, bool approved)
    {
        using var connection = connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            "UPDATE SeasonSummaries SET Status = @Status, ReviewedAt = @ReviewedAt WHERE Id = @Id",
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
        await connection.ExecuteAsync("DELETE FROM SeasonSummaries WHERE Id = @Id", new { Id = id });
    }
}
