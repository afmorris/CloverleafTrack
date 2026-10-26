using CloverleafTrack.DataAccess;
using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.DataAccess.Repositories;
using CloverleafTrack.Services;
using CloverleafTrack.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Season wrap poster PDF (QuestPDF). Community license: free for individuals, non-profits and organisations under $1M annual revenue.
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add services to the container.
builder.Services.AddControllersWithViews();
// Generate lowercase links (asp-controller/asp-action): the static export saves each URL as its own file, so a
// '/Meets' link next to '/meets' would produce a duplicate page (Meets.html and meets.html).
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddHttpContextAccessor();
builder.Services.Configure<CloverleafTrack.Web.Utilities.SeoUrlOptions>(builder.Configuration.GetSection("Seo"));
builder.Services.AddScoped<CloverleafTrack.Web.Utilities.ISeoUrlBuilder, CloverleafTrack.Web.Utilities.SeoUrlBuilder>();
builder.Services.AddScoped<IDbConnectionFactory>(_ => new SqlConnectionFactory(connectionString ?? string.Empty));
builder.Services.AddScoped<IAthleteRepository, AthleteRepository>();
builder.Services.AddScoped<ISeasonRepository, SeasonRepository>();
builder.Services.AddScoped<IPerformanceRepository, PerformanceRepository>();
builder.Services.AddScoped<IMeetRepository, MeetRepository>();
builder.Services.AddScoped<ILeaderboardRepository, LeaderboardRepository>();
builder.Services.AddScoped<IHomeRepository, HomeRepository>();
builder.Services.AddScoped<IScoringTemplateRepository, ScoringTemplateRepository>();
builder.Services.AddScoped<IMeetPlacingRepository, MeetPlacingRepository>();
builder.Services.AddScoped<ISeasonScoringRepository, SeasonScoringRepository>();
builder.Services.AddScoped<ISeasonWrapRepository, SeasonWrapRepository>();
builder.Services.AddScoped<IPerformanceAttemptRepository, PerformanceAttemptRepository>();
builder.Services.AddScoped<IPerformanceRoundRepository, PerformanceRoundRepository>();
builder.Services.AddScoped<IPerformanceBarRepository, PerformanceBarRepository>();

builder.Services.AddScoped<IAthleteService, AthleteService>();
builder.Services.AddScoped<ISeasonService, SeasonService>();
builder.Services.AddScoped<IMeetService, MeetService>();
builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<IScoringService, ScoringService>();
builder.Services.AddScoped<ISeasonWrapService, SeasonWrapService>();
builder.Services.AddScoped<IAthleteWrapService, AthleteWrapService>();
builder.Services.AddScoped<ISearchService, SearchService>();

builder.Services.AddScoped<IAdminAthleteRepository, AdminAthleteRepository>();
builder.Services.AddScoped<IAdminMeetRepository, AdminMeetRepository>();
builder.Services.AddScoped<IAdminPerformanceRepository, AdminPerformanceRepository>();
builder.Services.AddScoped<IAdminLocationRepository, AdminLocationRepository>();
builder.Services.AddScoped<IAdminEventRepository, AdminEventRepository>();
builder.Services.AddScoped<IAdminSeasonRepository, AdminSeasonRepository>();
builder.Services.AddScoped<IAdminScoringTemplateRepository, AdminScoringTemplateRepository>();
builder.Services.AddScoped<IAdminMeetParticipantRepository, AdminMeetParticipantRepository>();
builder.Services.AddScoped<IAdminMeetEntryRepository, AdminMeetEntryRepository>();
builder.Services.AddScoped<IAdminMeetPlacingRepository, AdminMeetPlacingRepository>();
builder.Services.AddScoped<IAdminMeetImportRepository, AdminMeetImportRepository>();
builder.Services.AddScoped<CloverleafTrack.Web.Import.MeetImportService>();
builder.Services.AddScoped<ISchoolRepository, SchoolRepository>();
builder.Services.AddScoped<IAdminSchoolRepository, AdminSchoolRepository>();
builder.Services.AddScoped<IAdminMeetTeamResultRepository, AdminMeetTeamResultRepository>();
builder.Services.AddScoped<IMeetSummaryRepository, MeetSummaryRepository>();

// AI meet summaries. The API key comes from user-secrets (Anthropic:ApiKey) or the ANTHROPIC_API_KEY env var — never appsettings.json.
builder.Services.AddSingleton(new ClaudeOptions(
    builder.Configuration["Anthropic:ApiKey"] ?? Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY"),
    builder.Configuration["Anthropic:Model"] ?? "claude-opus-5-5"));
builder.Services.AddHttpClient<ISummaryTextGenerator, ClaudeSummaryTextGenerator>(c => c.Timeout = TimeSpan.FromMinutes(2));
builder.Services.AddScoped<IMeetSummaryService, MeetSummaryService>();
builder.Services.AddScoped<IMeetSummaryLinkService, MeetSummaryLinkService>();
builder.Services.AddScoped<IAthleteSummaryRepository, AthleteSummaryRepository>();
builder.Services.AddScoped<ISeasonSummaryRepository, SeasonSummaryRepository>();
builder.Services.AddScoped<ISeasonSummaryService, SeasonSummaryService>();
builder.Services.AddScoped<IAthleteSummaryService, AthleteSummaryService>();
builder.Services.AddScoped<IAthleteSummaryLinker, AthleteSummaryLinker>();
builder.Services.AddScoped<IAthleteSummaryLinkService, AthleteSummaryLinkService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseSession();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "admin",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();