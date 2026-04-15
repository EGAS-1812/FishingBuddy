using FishingBuddy.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FishingBuddy.Controllers
{
    public class CatchRecordController : Controller
    {
        private readonly IFishingRepository _repository;

        public CatchRecordController(IFishingRepository repository)
        {
            _repository = repository;
        }

        public IActionResult Index()
        {
            return View(_repository.CatchRecords);
        }

        public IActionResult Details(int id)
        {
            var record = _repository.GetCatchRecordById(id);
            if (record == null) return NotFound();

            ViewBag.Fish = _repository.GetFishById(record.FishID);
            ViewBag.User = _repository.GetUserById(record.UserID);
            return View(record);
        }
    }
}
