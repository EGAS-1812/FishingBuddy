using FishingBuddy.Models;

namespace FishingBuddy.Models.Home;

public class HomeIndexViewModel
{
    public string SearchTerm { get; set; } = string.Empty;
    public FishSearchResultViewModel? SearchResult { get; set; }
    public bool SearchPerformed { get; set; }

    public int TechniqueCount { get; set; }
    public int BaitCount { get; set; }
    public int FishCount { get; set; }
    public int CatchRecordCount { get; set; }
    public int UserCount { get; set; }
    public int FishingSpotCount { get; set; }
    public IReadOnlyList<CatchRecord> RecentCatches { get; set; } = new List<CatchRecord>();
}
