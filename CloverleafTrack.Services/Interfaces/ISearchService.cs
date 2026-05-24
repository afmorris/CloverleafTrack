namespace CloverleafTrack.Services.Interfaces;

public record SearchRecord(string Type, string Label, string SubLabel, string Url);

public interface ISearchService
{
    Task<List<SearchRecord>> GetSearchIndexAsync();
}
