using FishingBuddy.Models.Search;

namespace FishingBuddy.Services.Search;

public interface IGlobalSearchService
{
    GlobalSearchViewModel Search(string? query, int maxResults = 40);
}
