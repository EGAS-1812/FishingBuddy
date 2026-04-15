using FishingBuddy.Models;

namespace FishingBuddy.Models.Home;

public class FishSearchResultViewModel
{
    public Fish Fish { get; set; } = new Fish();
    public Bait? RecommendedBait { get; set; }
    public Technique? RecommendedTechnique { get; set; }
    public IReadOnlyList<FishingSpot> RecommendedSpots { get; set; } = new List<FishingSpot>();
}
