namespace FishingBuddy.Models.Search;

public class GlobalSearchItemViewModel
{
    public string Category { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public int Score { get; set; }
}
