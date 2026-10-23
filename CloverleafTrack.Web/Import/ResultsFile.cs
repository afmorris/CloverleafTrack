using System.Text.Json;
using System.Text.Json.Serialization;

namespace CloverleafTrack.Web.Import;

/// <summary>
/// The meet-results file the admin Import screen reads (format version 1; documented in docs/meet-results-format.md).
/// It is prepared outside the site (in chat, from a scan / athletic.net / Hy-Tek) and reviewed before anything is written.
/// </summary>
public class ResultsFile
{
    public int Version { get; set; } = 1;
    /// <summary>Informational: where the numbers came from.</summary>
    public string? Source { get; set; }
    public string? Notes { get; set; }
    /// <summary>Optional safety check: when present it must match the meet the file is imported into.</summary>
    public ResultsFileMeet? Meet { get; set; }
    public List<ResultsFileResult> Results { get; set; } = new();
    public List<ResultsFileEntry> Entries { get; set; } = new();

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        Converters = { new FlexibleStringConverter() }
    };

    public static ResultsFile Parse(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) throw new ResultsFileException("The file is empty.");
        try
        {
            return JsonSerializer.Deserialize<ResultsFile>(json, Options)
                   ?? throw new ResultsFileException("The file is empty.");
        }
        catch (JsonException ex)
        {
            throw new ResultsFileException($"Not valid JSON: {ex.Message}");
        }
    }
}

public class ResultsFileMeet
{
    public int? Id { get; set; }
    public string? Name { get; set; }
    public string? Date { get; set; }
}

public class ResultsFileResult
{
    /// <summary>Boys / Girls / Mixed (also M, F, Male, Female, ...).</summary>
    public string? Gender { get; set; }
    /// <summary>Event name as the site spells it: "100M", "4x100M Relay", "Shot Put", "Pole Vault".</summary>
    public string? Event { get; set; }
    /// <summary>Individual events: "First Last".</summary>
    public string? Athlete { get; set; }
    /// <summary>Relays: one "First Last" per leg.</summary>
    public List<string>? Athletes { get; set; }
    /// <summary>"11.23", "2:05.4", "15' 10.25\"", "15-10.25". Leave empty when <see cref="Status"/> is set.</summary>
    public string? Mark { get; set; }
    /// <summary>NM, NH, DNF or DQ instead of a mark.</summary>
    public string? Status { get; set; }
    /// <summary>Overall place (invitationals) or the place against the only opponent (a dual).</summary>
    public int? Place { get; set; }
    /// <summary>Per-opponent places for duals and double duals.</summary>
    public List<ResultsFilePlacing>? Placings { get; set; }
    public string? Note { get; set; }
}

public class ResultsFilePlacing
{
    public string? Opponent { get; set; }
    public int Place { get; set; }
}

public class ResultsFileEntry
{
    public string? Gender { get; set; }
    public string? Event { get; set; }
    public string? Athlete { get; set; }
    public List<string>? Athletes { get; set; }
}

public class ResultsFileException(string message) : Exception(message);

/// <summary>Reads a JSON string, or a bare number (11.23), as text so marks can be written either way.</summary>
public sealed class FlexibleStringConverter : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => reader.TokenType switch
    {
        JsonTokenType.String => reader.GetString(),
        JsonTokenType.Number => reader.GetDouble().ToString(System.Globalization.CultureInfo.InvariantCulture),
        JsonTokenType.Null => null,
        _ => throw new JsonException($"Expected text or a number, found {reader.TokenType}.")
    };

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options) => writer.WriteStringValue(value);
}
