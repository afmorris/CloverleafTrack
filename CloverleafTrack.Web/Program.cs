using CloverleafTrack.DataAccess;
using CloverleafTrack.DataAccess.Interfaces;
using CloverleafTrack.DataAccess.Repositories;
using CloverleafTrack.Services;
using CloverleafTrack.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IDbConnectionFactory>(_ => new SqlConnectionFactory(connectionString ?? string.Empty));
builder.Services.AddScoped<IAthleteRepository, AthleteRepository>();
builder.Services.AddScoped<ISeasonRepository, SeasonRepository>();
builder.Services.AddScoped<IPerformanceRepository, PerformanceRepository>();
builder.Services.AddScoped<IMeetRepository, MeetRepository>();
builder.Services.AddScoped<ILeaderboardRepository, LeaderboardRepository>();
builder.Services.AddScoped<IHomeRepository, HomeRepository>();

builder.Services.AddScoped<IAthleteService, AthleteService>();
builder.Services.AddScoped<ISeasonService, SeasonService>();
builder.Services.AddScoped<IMeetService, MeetService>();
builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();
builder.Services.AddScoped<IHomeService, HomeService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();