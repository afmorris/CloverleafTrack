using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace CloverleafTrack.Services;

/// <summary>Serialization shared by the meet and athlete fact sheets so both strip nulls and hash the same way.</summary>
internal static class FactSheetJson
{
    private static readonly JsonSerializerOptions Options = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    /// <summary>DefaultIgnoreCondition does not apply to dictionary entries, so strip nulls explicitly (saves tokens, avoids "pr":null noise).</summary>
    public static string Serialize(object sheet)
    {
        var node = JsonSerializer.SerializeToNode(sheet, Options)!;
        StripNulls(node);
        return node.ToJsonString(Options);
    }

    public static string Hash(string version, string systemPrompt, string factSheet) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(version + "\n" + systemPrompt + "\n" + factSheet))).ToLowerInvariant();

    private static void StripNulls(JsonNode? node)
    {
        switch (node)
        {
            case JsonObject obj:
                foreach (var key in obj.Where(kv => kv.Value is null).Select(kv => kv.Key).ToList()) obj.Remove(key);
                foreach (var value in obj.Select(kv => kv.Value)) StripNulls(value);
                break;
            case JsonArray arr:
                foreach (var item in arr) StripNulls(item);
                break;
        }
    }
}
