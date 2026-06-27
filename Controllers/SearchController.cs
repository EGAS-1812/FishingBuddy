using FishingBuddy.Services.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FishingBuddy.Controllers;

[AllowAnonymous]
public class SearchController(IGlobalSearchService globalSearchService) : Controller
{
    [HttpGet]
    public IActionResult Index(string? q)
    {
        var model = globalSearchService.Search(q);
        return View(model);
    }
}
