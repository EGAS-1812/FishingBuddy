using FishingBuddy.Models;
using FishingBuddy.Models.Home;
using FishingBuddy.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FishingBuddy.Controllers
{
    public class HomeController : Controller
    {
        private readonly IFishingRepository _repository;

        public HomeController(IFishingRepository repository)
        {
            _repository = repository;
        }

        public IActionResult Index(string? fishName)
        {
            var model = new HomeIndexViewModel
            {
                SearchTerm = fishName?.Trim() ?? string.Empty,
                SearchPerformed = !string.IsNullOrWhiteSpace(fishName),
                TechniqueCount = _repository.Techniques.Count,
                BaitCount = _repository.Baits.Count,
                FishCount = _repository.Fish.Count,
                CatchRecordCount = _repository.CatchRecords.Count,
                UserCount = _repository.Users.Count,
                FishingSpotCount = _repository.FishingSpots.Count,
                RecentCatches = _repository.CatchRecords
                    .OrderByDescending(c => c.CatchDate)
                    .Take(3)
                    .ToList()
            };

            if (model.SearchPerformed)
            {
                var searchedFish = _repository.Fish
                    .FirstOrDefault(f => f.SpeciesName.Equals(model.SearchTerm, StringComparison.OrdinalIgnoreCase))
                    ?? _repository.Fish.FirstOrDefault(f => f.SpeciesName.Contains(model.SearchTerm, StringComparison.OrdinalIgnoreCase));

                if (searchedFish != null)
                {
                    model.SearchResult = new FishSearchResultViewModel
                    {
                        Fish = searchedFish,
                        RecommendedBait = _repository.GetBaitById(searchedFish.FavouriteBaitID),
                        RecommendedTechnique = searchedFish.PreferredMethod,
                        RecommendedSpots = _repository.FishingSpots
                            .Where(s => s.MostLikelyCatch.Any(f => f.FishID == searchedFish.FishID))
                            .ToList()
                    };
                }
            }

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
