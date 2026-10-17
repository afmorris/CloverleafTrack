using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.Models;
using CloverleafTrack.Models.Enums;
using Dapper;

namespace CloverleafTrack.DataAccess.Repositories;

public class MeetSummaryRepository(IDbConnectionFactory connectionFactory) : IMeetSummaryRepository
{
    public async Task<MeetSummary?> GetForMeetAsync(int meetId)
    {
        using var connection = connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<MeetSummary>(
            "SELECT * FROM MeetSummaries WHERE MeetId = @MeetId", new { MeetId = meetId });
    }

    public async Task<List<MeetSummary>> GetForMeetsAsync(IEnumerable<int> meetIds)
    {
        var ids = meetIds.ToList();
        if (ids.Count == 0) return new List<MeetSummary>();

        using var connection = connectionFactory.CreateConnection();
        var rows = await connection.QueryAsync<MeetSummary>(
            "SELECT * FROM MeetSummaries WHERE MeetId IN @Ids", new { Ids = ids });
        return rows.ToList();
    }

    public async Task SaveDraftAsync(int meetId, string text, string inputHash, string model)
    {
        using var connection = connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE MeetSummaries
            SET SummaryText = @Text, InputHash = @InputHash, Model = @Model, Status = @Draft,
                GeneratedAt = GETUTCDATE(), ReviewedAt = NULL
            WHERE MeetId = @MeetId;
            IF @@ROWCOUNT = 0
                INSERT INTO MeetSummaries (MeetId, SummaryText, InputHash, Model, Status)
                VALUES (@MeetId, @Text, @InputHash, @Model, @Draft);";
        await connection.ExecuteAsync(sql, new
        {
            MeetId = meetId, Text = text, InputHash = inputHash, Model = model, Draft = (short)SummaryStatus.Draft
        });
    }

    public async Task UpdateTextAsync(int id, string text)
    {
        using var connection = connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            "UPDATE MeetSummaries SET SummaryText = @Text, Status = @Draft, ReviewedAt = NULL WHERE Id = @Id",
            new { Id = id, Text = text, Draft = (short)SummaryStatus.Draft });
    }

    public async Task SetApprovedAsync(int id, bool approved)
    {
        using var connection = connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            "UPDATE MeetSummaries SET Status = @Status, ReviewedAt = @ReviewedAt WHERE Id = @Id",
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
        await connection.ExecuteAsync("DELETE FROM MeetSummaries WHERE Id = @Id", new { Id = id });
    }
}
