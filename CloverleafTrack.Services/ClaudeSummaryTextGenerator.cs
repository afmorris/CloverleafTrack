using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CloverleafTrack.Services.Interfaces;

namespace CloverleafTrack.Services;

/// <summary>Config for the Anthropic Messages API. ApiKey comes from user-secrets / environment, never appsettings.json.</summary>
public record ClaudeOptions(string? ApiKey, string Model);

/// <summary>Calls the Anthropic Messages API directly over HTTP (one request, no tools, no streaming).</summary>
public class ClaudeSummaryTextGenerator(HttpClient http, ClaudeOptions options) : ISummaryTextGenerator
{
    public async Task<GeneratedText> GenerateAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(options.ApiKey))
        {
            throw new InvalidOperationException(
                "No Anthropic API key configured. Set Anthropic:ApiKey (dotnet user-secrets) or the ANTHROPIC_API_KEY environment variable.");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages")
        {
            Content = JsonContent.Create(new
            {
                model = options.Model,
                max_tokens = 1024,
                system = systemPrompt,
                messages = new[] { new { role = "user", content = userPrompt } }
            })
        };
        request.Headers.Add("x-api-key", options.ApiKey);
        request.Headers.Add("anthropic-version", "2023-06-01");

        using var response = await http.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Anthropic API returned {(int)response.StatusCode}: {body}");
        }

        var parsed = JsonSerializer.Deserialize<MessagesResponse>(body)
                     ?? throw new InvalidOperationException("Empty response from Anthropic API.");
        var text = string.Concat(parsed.Content.Where(c => c.Type == "text").Select(c => c.Text)).Trim();
        if (text.Length == 0)
        {
            throw new InvalidOperationException($"Anthropic API returned no text (stop_reason: {parsed.StopReason}).");
        }

        return new GeneratedText(text, parsed.Model ?? options.Model);
    }

    private class MessagesResponse
    {
        [JsonPropertyName("model")] public string? Model { get; set; }
        [JsonPropertyName("stop_reason")] public string? StopReason { get; set; }
        [JsonPropertyName("content")] public List<ContentBlock> Content { get; set; } = new();
    }

    private class ContentBlock
    {
        [JsonPropertyName("type")] public string Type { get; set; } = string.Empty;
        [JsonPropertyName("text")] public string? Text { get; set; }
    }
}
