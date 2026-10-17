namespace CloverleafTrack.Services.Interfaces;

public record GeneratedText(string Text, string Model);

/// <summary>Thin seam over the LLM call so summary logic can be tested without the network.</summary>
public interface ISummaryTextGenerator
{
    Task<GeneratedText> GenerateAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default);
}
