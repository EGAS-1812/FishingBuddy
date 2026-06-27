using FishingBuddy.Models.Ai;

namespace FishingBuddy.Services.Ai;

public interface IAiFishDraftService
{
    Task<AiFishDraftResultViewModel> BuildDraftAsync(string prompt, CancellationToken cancellationToken = default);
}
