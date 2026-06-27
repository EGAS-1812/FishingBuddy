namespace FishingBuddy.Models.Search;

public class GlobalSearchViewModel
{
    public string Query { get; set; } = string.Empty;
    public bool SearchPerformed { get; set; }
    public int TotalResults { get; set; }
    public IReadOnlyList<GlobalSearchItemViewModel> Results { get; set; } = new List<GlobalSearchItemViewModel>();
}
