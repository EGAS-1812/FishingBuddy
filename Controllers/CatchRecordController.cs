using FishingBuddy.Models;
using FishingBuddy.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

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

        public IActionResult Manage()
        {
            return View(_repository.CatchRecords.OrderByDescending(c => c.CatchDate).ToList());
        }

        public IActionResult Details(int id)
        {
            var record = _repository.GetCatchRecordById(id);
            if (record == null) return NotFound();

            ViewBag.Fish = _repository.GetFishById(record.FishID);
            ViewBag.User = _repository.GetUserById(record.UserID);
            return View(record);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Users = new SelectList(_repository.Users, "UserID", "Username");
            ViewBag.Fish = new SelectList(_repository.Fish, "FishID", "SpeciesName");
            return View(new CatchRecord { CatchDate = DateTime.Today });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CatchRecord catchRecord)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Users = new SelectList(_repository.Users, "UserID", "Username");
                ViewBag.Fish = new SelectList(_repository.Fish, "FishID", "SpeciesName");
                return View(catchRecord);
            }
            _repository.AddCatchRecord(catchRecord);
            return RedirectToAction(nameof(Manage));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var record = _repository.GetCatchRecordById(id);
            if (record == null) return NotFound();
            ViewBag.Users = new SelectList(_repository.Users, "UserID", "Username", record.UserID);
            ViewBag.Fish = new SelectList(_repository.Fish, "FishID", "SpeciesName", record.FishID);
            return View(record);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, CatchRecord catchRecord)
        {
            if (id != catchRecord.CatchID) return NotFound();
            if (!ModelState.IsValid)
            {
                ViewBag.Users = new SelectList(_repository.Users, "UserID", "Username", catchRecord.UserID);
                ViewBag.Fish = new SelectList(_repository.Fish, "FishID", "SpeciesName", catchRecord.FishID);
                return View(catchRecord);
            }
            _repository.UpdateCatchRecord(catchRecord);
            return RedirectToAction(nameof(Manage));
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var record = _repository.GetCatchRecordById(id);
            if (record == null) return NotFound();
            return View(record);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _repository.DeleteCatchRecord(id);
            return RedirectToAction(nameof(Manage));
        }
    }
}
