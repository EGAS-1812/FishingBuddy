using FishingBuddy.Models.Search;
using FishingBuddy.Repositories;

namespace FishingBuddy.Services.Search;

public class GlobalSearchService(IFishingRepository repository) : IGlobalSearchService
{
    private static readonly IReadOnlyList<(string Title, string Subtitle, string Url)> MenuPages =
    [
        ("Pocetna", "Pocetna stranica i statistika", "/"),
        ("Ribolovna mjesta", "Pregled svih mjesta", "/destinations/spots"),
        ("Ribe", "Katalog vrsta riba", "/catalog/fish"),
        ("Evidencija ulova", "Zapisi ulova", "/CatchRecord"),
        ("Mamci", "Katalog mamaca", "/catalog/baits"),
        ("Tehnike", "Ribolovne tehnike", "/Technique"),
        ("Ribari", "Clanovi zajednice", "/community/anglers"),
        ("AI Fish Draft", "AI asistirano popunjavanje podataka o ribi", "/Ai/FishDraft"),
        ("Privatnost", "Pravila privatnosti", "/Home/Privacy")
    ];

    public GlobalSearchViewModel Search(string? query, int maxResults = 40)
    {
        var normalized = (query ?? string.Empty).Trim();
        var model = new GlobalSearchViewModel
        {
            Query = normalized,
            SearchPerformed = !string.IsNullOrWhiteSpace(normalized)
        };

        if (!model.SearchPerformed)
        {
            return model;
        }

        var items = new List<GlobalSearchItemViewModel>();

        items.AddRange(MenuPages
            .Select(page => CreateMatch("Menu/Page", page.Title, page.Subtitle, page.Url, normalized))
            .Where(item => item != null)
            .Select(item => item!));

        items.AddRange(repository.Fish
            .Select(f => CreateMatch("Fish", f.SpeciesName, $"Sezona: {f.CatchSeason}; Meso: {f.FleshColor}", $"/Fish/Details/{f.FishID}", normalized))
            .Where(item => item != null)
            .Select(item => item!));

        items.AddRange(repository.Baits
            .Select(b => CreateMatch("Bait", b.BaitName, $"Tip: {b.BaitType}; Priprema: {b.PreparationMethod}", $"/Bait/Details/{b.BaitID}", normalized))
            .Where(item => item != null)
            .Select(item => item!));

        items.AddRange(repository.Techniques
            .Select(t => CreateMatch("Technique", t.TechniqueName, t.PerformanceNote, $"/Technique/Details/{t.TechniqueID}", normalized))
            .Where(item => item != null)
            .Select(item => item!));

        items.AddRange(repository.Users
            .Select(u => CreateMatch("User", u.Username, u.Email, $"/User/Details/{u.UserID}", normalized))
            .Where(item => item != null)
            .Select(item => item!));

        items.AddRange(repository.FishingSpots
            .Select(s => CreateMatch("FishingSpot", s.SpotName, $"Regija: {s.Region}", $"/FishingSpot/Details/{s.SpotID}", normalized))
            .Where(item => item != null)
            .Select(item => item!));

        items.AddRange(repository.CatchRecords
            .Select(c => CreateMatch("CatchRecord", $"Ulov #{c.CatchID}", $"{c.Location}; {c.CatchDate:dd.MM.yyyy}", $"/CatchRecord/Details/{c.CatchID}", normalized))
            .Where(item => item != null)
            .Select(item => item!));

        model.Results = items
            .OrderByDescending(i => i.Score)
            .ThenBy(i => i.Category)
            .ThenBy(i => i.Title)
            .Take(maxResults)
            .ToList();

        model.TotalResults = model.Results.Count;
        return model;
    }

    private static GlobalSearchItemViewModel? CreateMatch(string category, string title, string subtitle, string url, string term)
    {
        var titleScore = ComputeScore(title, term);
        var subtitleScore = ComputeScore(subtitle, term);
        var bestScore = Math.Max(titleScore, subtitleScore);

        if (bestScore <= 0)
        {
            return null;
        }

        return new GlobalSearchItemViewModel
        {
            Category = category,
            Title = title,
            Subtitle = subtitle,
            Url = url,
            Score = bestScore
        };
    }

    private static int ComputeScore(string source, string term)
    {
        if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(term))
        {
            return 0;
        }

        if (source.Equals(term, StringComparison.OrdinalIgnoreCase))
        {
            return 120;
        }

        if (source.StartsWith(term, StringComparison.OrdinalIgnoreCase))
        {
            return 100;
        }

        var index = source.IndexOf(term, StringComparison.OrdinalIgnoreCase);
        if (index < 0)
        {
            return 0;
        }

        return Math.Max(50 - index, 15);
    }
}
