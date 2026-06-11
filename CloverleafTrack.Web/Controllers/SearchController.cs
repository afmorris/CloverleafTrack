using Microsoft.AspNetCore.Mvc;
using CloverleafTrack.Services.Interfaces;

namespace CloverleafTrack.Web.Controllers;

public class SearchController(ISearchService searchService, IWebHostEnvironment env) : Controller
{
    [HttpGet("/search-index.json")]
    [ResponseCache(Duration = 300, VaryByHeader = "Accept-Encoding")]
    public async Task<IActionResult> Index()
    {
        var filePath = Path.Combine(env.WebRootPath, "search-index.json");
        if (System.IO.File.Exists(filePath))
            return PhysicalFile(filePath, "application/json");

        var records = await searchService.GetSearchIndexAsync();
        return Json(records, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        });
    }

    [HttpGet("/static/search-index.json")]
    [ResponseCache(Duration = 300, VaryByHeader = "Accept-Encoding")]
    public async Task<IActionResult> StaticExport()
    {
        var records = await searchService.GetSearchIndexAsync();
        var staticRecords = records.Select(r => r with { Url = r.Url + ".html" }).ToList();
        return Json(staticRecords, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        });
    }
}
