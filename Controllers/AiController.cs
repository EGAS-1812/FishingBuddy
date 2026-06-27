using FishingBuddy.Models.Ai;
using FishingBuddy.Services.Ai;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FishingBuddy.Controllers;

public class AiController(IAiFishDraftService aiFishDraftService) : Controller
{
    [HttpGet]
    [AllowAnonymous]
    public IActionResult FishDraft()
    {
        return View(new AiFishDraftPageViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> FishDraft(AiFishDraftPageViewModel model, CancellationToken cancellationToken)
    {
        var prompt = model.Prompt?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(prompt))
        {
            model.ErrorMessage = "Unesite prompt prije slanja.";
            return View(model);
        }

        model.Result = await aiFishDraftService.BuildDraftAsync(prompt, cancellationToken);
        model.HasResult = true;
        return View(model);
    }
}
