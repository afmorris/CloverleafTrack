using System.Text.Json;
using CloverleafTrack.DataAccess;
using CloverleafTrack.DataAccess.Repositories;
using CloverleafTrack.Services;

var connectionString = args.Length > 0
    ? args[0]
    : Environment.GetEnvironmentVariable("SEARCH_DB_CONNECTION");

var outputPath = args.Length > 1 ? args[1] : null;

if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.Error.WriteLine("Usage: SearchGenerator <connection-string> [output-path]");
    Console.Error.WriteLine("       Or set SEARCH_DB_CONNECTION environment variable.");
    return 1;
}

// Manually wire the dependency graph — no DI container needed.
var connectionFactory = new SqlConnectionFactory(connectionString);
var athleteRepo      = new AthleteRepository(connectionFactory);
var seasonRepo       = new SeasonRepository(connectionFactory);
var meetRepo         = new MeetRepository(connectionFactory);
var meetPlacingRepo  = new MeetPlacingRepository(connectionFactory);
var leaderboardRepo  = new LeaderboardRepository(connectionFactory);
var performanceRepo  = new PerformanceRepository(connectionFactory);

var athleteService     = new AthleteService(athleteRepo);
var seasonService      = new SeasonService(seasonRepo, performanceRepo, meetRepo);
var meetService        = new MeetService(meetRepo, meetPlacingRepo);
var leaderboardService = new LeaderboardService(leaderboardRepo);
var searchService      = new SearchService(athleteService, meetService, leaderboardService, seasonService);

var records = await searchService.GetSearchIndexAsync();

var json = JsonSerializer.Serialize(records, new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = false
});

if (outputPath is not null)
{
    var dir = Path.GetDirectoryName(outputPath);
    if (!string.IsNullOrEmpty(dir))
        Directory.CreateDirectory(dir);
    await File.WriteAllTextAsync(outputPath, json);
    Console.WriteLine($"Wrote {records.Count} records to {outputPath}");
}
else
{
    Console.Write(json);
}

return 0;
