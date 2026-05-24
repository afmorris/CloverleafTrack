using Microsoft.AspNetCore.Mvc;
using CloverleafTrack.Services.Interfaces;

namespace CloverleafTrack.Web.Controllers;

public class SearchController(ISearchService searchService) : Controller
{
    [HttpGet("/search-index.json")]
    [ResponseCache(Duration = 300, VaryByHeader = "Accept-Encoding")]
    public async Task<IActionResult> Index()
    {
        var records = await searchService.GetSearchIndexAsync();
        return Json(records, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        });
    }
}
