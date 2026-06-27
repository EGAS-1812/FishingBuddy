using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FishingBuddy.Models;
using FishingBuddy.Models.Ai;
using FishingBuddy.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FishingBuddy.Services.Ai;

public class AiFishDraftService(
    HttpClient httpClient,
    IConfiguration configuration,
    IFishingRepository repository,
    ILogger<AiFishDraftService> logger) : IAiFishDraftService
{
    private static readonly Dictionary<string, SpeciesProfile> SpeciesProfiles =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["swordfish"] = new(Season.Summer, FishFlesh.Red, "Trolling", "Srdela", FReelType.Troll, FRodAction.Heavy, FLineType.Braided),
            ["iglun"] = new(Season.Summer, FishFlesh.Red, "Trolling", "Srdela", FReelType.Troll, FRodAction.Heavy, FLineType.Braided),
            ["sea bass"] = new(Season.Winter, FishFlesh.White, "Spinning", "Crv", FReelType.Spinning, FRodAction.Medium, FLineType.Braided),
            ["brancin"] = new(Season.Winter, FishFlesh.White, "Spinning", "Crv", FReelType.Spinning, FRodAction.Medium, FLineType.Braided),
            ["tuna"] = new(Season.Summer, FishFlesh.Red, "Trolling", "Varalica", FReelType.Troll, FRodAction.Heavy, FLineType.Braided),
            ["skuša"] = new(Season.Summer, FishFlesh.Blue, "Spinning", "Srdela", FReelType.Spinning, FRodAction.MediumLight, FLineType.Nylon),
            ["mackerel"] = new(Season.Summer, FishFlesh.Blue, "Spinning", "Srdela", FReelType.Spinning, FRodAction.MediumLight, FLineType.Nylon)
        };

    public async Task<AiFishDraftResultViewModel> BuildDraftAsync(string prompt, CancellationToken cancellationToken = default)
    {
        var normalizedPrompt = (prompt ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalizedPrompt))
        {
            return new AiFishDraftResultViewModel
            {
                Notes = "Prompt is empty. Provide a short description of fish, season, bait, and technique."
            };
        }

        var apiKey = configuration["Ai:OpenAi:ApiKey"];
        var speciesHint = ExtractSpeciesFromPrompt(normalizedPrompt);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return BuildHeuristicDraft(normalizedPrompt, speciesHint);
        }

        try
        {
            var modelDraft = await BuildModelDraftAsync(normalizedPrompt, apiKey, cancellationToken);
            if (!string.IsNullOrWhiteSpace(speciesHint) && string.IsNullOrWhiteSpace(modelDraft.SpeciesName))
            {
                modelDraft.SpeciesName = speciesHint;
            }

            EnrichWithSpeciesKnowledge(modelDraft);
            EnsureRequiredDefaults(modelDraft);
            return modelDraft;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "OpenAI draft request failed. Falling back to local heuristic parser.");
            var fallback = BuildHeuristicDraft(normalizedPrompt, speciesHint);
            fallback.Notes = string.IsNullOrWhiteSpace(fallback.Notes)
                ? "Model request failed, local heuristic parser was used."
                : $"{fallback.Notes} Model request failed, local heuristic parser was used.";
            return fallback;
        }
    }

    private async Task<AiFishDraftResultViewModel> BuildModelDraftAsync(string prompt, string apiKey, CancellationToken cancellationToken)
    {
        var endpoint = configuration["Ai:OpenAi:Endpoint"]?.Trim();
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            endpoint = "https://api.openai.com/v1/chat/completions";
        }

        var model = configuration["Ai:OpenAi:Model"]?.Trim();
        if (string.IsNullOrWhiteSpace(model))
        {
            model = "gpt-4o-mini";
        }

        var payload = new
        {
            model,
            temperature = 0.2,
            messages = new object[]
            {
                new
                {
                    role = "system",
                    content = "Extract fish data from user prompt. Return JSON only with keys: speciesName, catchSeason, fleshColor, baitName, techniqueName, notes. Use enum names for catchSeason (Spring,Summer,Autumn,Winter) and fleshColor (White,Red,Blue)."
                },
                new
                {
                    role = "user",
                    content = prompt
                }
            }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseText = await response.Content.ReadAsStringAsync(cancellationToken);
        var completion = JsonDocument.Parse(responseText);
        var rawContent = completion.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? string.Empty;

        var json = ExtractJson(rawContent);
        var draftDoc = JsonDocument.Parse(json);
        var root = draftDoc.RootElement;

        var result = new AiFishDraftResultViewModel
        {
            SpeciesName = ReadString(root, "speciesName"),
            CatchSeason = ParseSeason(ReadString(root, "catchSeason")),
            FleshColor = ParseFlesh(ReadString(root, "fleshColor")),
            SuggestedBaitName = ReadString(root, "baitName"),
            SuggestedTechniqueName = ReadString(root, "techniqueName"),
            Notes = ReadString(root, "notes"),
            Source = "model",
            Model = model
        };

        MatchRelatedEntityIds(result);

        if (string.IsNullOrWhiteSpace(result.SpeciesName))
        {
            var speciesHint = ExtractSpeciesFromPrompt(prompt);
            result.SpeciesName = BuildHeuristicDraft(prompt, speciesHint).SpeciesName;
        }

        return result;
    }

    private AiFishDraftResultViewModel BuildHeuristicDraft(string prompt, string speciesHint)
    {
        var result = new AiFishDraftResultViewModel
        {
            SpeciesName = !string.IsNullOrWhiteSpace(speciesHint) ? speciesHint : GuessSpecies(prompt),
            CatchSeason = ParseSeason(prompt),
            FleshColor = ParseFlesh(prompt),
            Source = "heuristic",
            Model = "local"
        };

        var bait = repository.Baits
            .FirstOrDefault(b => prompt.Contains(b.BaitName, StringComparison.OrdinalIgnoreCase));
        if (bait != null)
        {
            result.FavouriteBaitID = bait.BaitID;
            result.SuggestedBaitName = bait.BaitName;
        }

        var technique = repository.Techniques
            .FirstOrDefault(t => prompt.Contains(t.TechniqueName, StringComparison.OrdinalIgnoreCase));
        if (technique != null)
        {
            result.PreferredMethodID = technique.TechniqueID;
            result.SuggestedTechniqueName = technique.TechniqueName;
        }

        EnrichWithSpeciesKnowledge(result);
        EnsureRequiredDefaults(result);

        if (string.IsNullOrWhiteSpace(result.Notes))
        {
            result.Notes = "Draft generated from local parser and species knowledge. Review values before saving.";
        }

        return result;
    }

    private void EnrichWithSpeciesKnowledge(AiFishDraftResultViewModel result)
    {
        if (string.IsNullOrWhiteSpace(result.SpeciesName))
        {
            return;
        }

        var fromRepository = repository.Fish.FirstOrDefault(f =>
            string.Equals(f.SpeciesName, result.SpeciesName, StringComparison.OrdinalIgnoreCase))
            ?? repository.Fish.FirstOrDefault(f =>
                f.SpeciesName.Contains(result.SpeciesName, StringComparison.OrdinalIgnoreCase)
                || result.SpeciesName.Contains(f.SpeciesName, StringComparison.OrdinalIgnoreCase));

        if (fromRepository != null)
        {
            result.SpeciesName = fromRepository.SpeciesName;
            result.CatchSeason ??= fromRepository.CatchSeason;
            result.FleshColor ??= fromRepository.FleshColor;

            if (!result.FavouriteBaitID.HasValue)
            {
                result.FavouriteBaitID = fromRepository.FavouriteBaitID;
                var bait = repository.GetBaitById(fromRepository.FavouriteBaitID);
                if (bait != null)
                {
                    result.SuggestedBaitName = bait.BaitName;
                }
            }

            if (!result.PreferredMethodID.HasValue)
            {
                result.PreferredMethodID = fromRepository.PreferredMethodID;
                var technique = repository.GetTechniqueById(fromRepository.PreferredMethodID);
                if (technique != null)
                {
                    result.SuggestedTechniqueName = technique.TechniqueName;
                }
            }

            // Equipment from this fish
            result.SuggestedReelType ??= fromRepository.Equipment.FReel.Type;
            result.SuggestedReelSize ??= fromRepository.Equipment.FReel.Size;
            result.SuggestedRodAction ??= fromRepository.Equipment.FRod.Action;
            result.SuggestedRodLengthMeters ??= fromRepository.Equipment.FRod.LengthMeters;
            result.SuggestedRodMinWeightGrams ??= fromRepository.Equipment.FRod.MinWeightRatingGrams;
            result.SuggestedRodMaxWeightGrams ??= fromRepository.Equipment.FRod.MaxWeightRatingGrams;
            result.SuggestedLineType ??= fromRepository.Equipment.FLine.Type;
            result.SuggestedLineThicknessMm ??= fromRepository.Equipment.FLine.ThicknessMm;

            // Fishing spots where this species can be caught
            if (result.SuggestedSpotNames.Count == 0)
            {
                result.SuggestedSpotNames = repository.FishingSpots
                    .Where(s => s.MostLikelyCatch.Any(f => f.FishID == fromRepository.FishID))
                    .Take(3)
                    .Select(s => s.SpotName)
                    .ToList();
            }

            return;
        }

        if (TryGetSpeciesProfile(result.SpeciesName, out var profile))
        {
            result.CatchSeason ??= profile.CatchSeason;
            result.FleshColor ??= profile.FleshColor;

            if (string.IsNullOrWhiteSpace(result.SuggestedTechniqueName))
            {
                result.SuggestedTechniqueName = profile.TechniqueName;
            }

            if (string.IsNullOrWhiteSpace(result.SuggestedBaitName))
            {
                result.SuggestedBaitName = profile.BaitName;
            }

            MatchRelatedEntityIds(result);

            // Equipment: find a repo fish that uses this technique, copy its gear
            if (result.PreferredMethodID.HasValue)
            {
                var sampleFish = repository.Fish
                    .FirstOrDefault(f => f.PreferredMethodID == result.PreferredMethodID.Value);
                if (sampleFish != null)
                {
                    result.SuggestedReelType ??= sampleFish.Equipment.FReel.Type;
                    result.SuggestedReelSize ??= sampleFish.Equipment.FReel.Size;
                    result.SuggestedRodAction ??= sampleFish.Equipment.FRod.Action;
                    result.SuggestedRodLengthMeters ??= sampleFish.Equipment.FRod.LengthMeters;
                    result.SuggestedRodMinWeightGrams ??= sampleFish.Equipment.FRod.MinWeightRatingGrams;
                    result.SuggestedRodMaxWeightGrams ??= sampleFish.Equipment.FRod.MaxWeightRatingGrams;
                    result.SuggestedLineType ??= sampleFish.Equipment.FLine.Type;
                    result.SuggestedLineThicknessMm ??= sampleFish.Equipment.FLine.ThicknessMm;
                }
            }
        }
    }

    private void EnsureRequiredDefaults(AiFishDraftResultViewModel result)
    {
        if (!result.CatchSeason.HasValue)
        {
            result.CatchSeason = Season.Summer;
        }

        if (!result.FleshColor.HasValue)
        {
            result.FleshColor = FishFlesh.White;
        }

        if (!result.FavouriteBaitID.HasValue)
        {
            var bait = repository.Baits
                .OrderBy(b => b.BaitID)
                .FirstOrDefault();
            if (bait != null)
            {
                result.FavouriteBaitID = bait.BaitID;
                result.SuggestedBaitName = bait.BaitName;
            }
        }

        if (!result.PreferredMethodID.HasValue)
        {
            var technique = repository.Techniques
                .OrderBy(t => t.TechniqueID)
                .FirstOrDefault();
            if (technique != null)
            {
                result.PreferredMethodID = technique.TechniqueID;
                result.SuggestedTechniqueName = technique.TechniqueName;
            }
        }

        if (!result.SuggestedReelType.HasValue)
        {
            var sampleFish = result.PreferredMethodID.HasValue
                ? repository.Fish.FirstOrDefault(f => f.PreferredMethodID == result.PreferredMethodID.Value)
                : repository.Fish.FirstOrDefault();
            if (sampleFish != null)
            {
                result.SuggestedReelType ??= sampleFish.Equipment.FReel.Type;
                result.SuggestedReelSize ??= sampleFish.Equipment.FReel.Size;
                result.SuggestedRodAction ??= sampleFish.Equipment.FRod.Action;
                result.SuggestedRodLengthMeters ??= sampleFish.Equipment.FRod.LengthMeters;
                result.SuggestedRodMinWeightGrams ??= sampleFish.Equipment.FRod.MinWeightRatingGrams;
                result.SuggestedRodMaxWeightGrams ??= sampleFish.Equipment.FRod.MaxWeightRatingGrams;
                result.SuggestedLineType ??= sampleFish.Equipment.FLine.Type;
                result.SuggestedLineThicknessMm ??= sampleFish.Equipment.FLine.ThicknessMm;
            }
        }
    }

    private static bool TryGetSpeciesProfile(string speciesName, out SpeciesProfile profile)
    {
        var normalized = speciesName.Trim();
        if (SpeciesProfiles.TryGetValue(normalized, out profile))
        {
            return true;
        }

        foreach (var pair in SpeciesProfiles)
        {
            if (normalized.Contains(pair.Key, StringComparison.OrdinalIgnoreCase)
                || pair.Key.Contains(normalized, StringComparison.OrdinalIgnoreCase))
            {
                profile = pair.Value;
                return true;
            }
        }

        profile = default;
        return false;
    }

    private void MatchRelatedEntityIds(AiFishDraftResultViewModel result)
    {
        if (!result.FavouriteBaitID.HasValue && !string.IsNullOrWhiteSpace(result.SuggestedBaitName))
        {
            var bait = repository.Baits.FirstOrDefault(b =>
                string.Equals(b.BaitName, result.SuggestedBaitName, StringComparison.OrdinalIgnoreCase))
                ?? repository.Baits.FirstOrDefault(b =>
                    b.BaitName.Contains(result.SuggestedBaitName, StringComparison.OrdinalIgnoreCase));

            if (bait != null)
            {
                result.FavouriteBaitID = bait.BaitID;
                result.SuggestedBaitName = bait.BaitName;
            }
        }

        if (!result.PreferredMethodID.HasValue && !string.IsNullOrWhiteSpace(result.SuggestedTechniqueName))
        {
            var technique = repository.Techniques.FirstOrDefault(t =>
                string.Equals(t.TechniqueName, result.SuggestedTechniqueName, StringComparison.OrdinalIgnoreCase))
                ?? repository.Techniques.FirstOrDefault(t =>
                    t.TechniqueName.Contains(result.SuggestedTechniqueName, StringComparison.OrdinalIgnoreCase));

            if (technique != null)
            {
                result.PreferredMethodID = technique.TechniqueID;
                result.SuggestedTechniqueName = technique.TechniqueName;
            }
        }
    }

    private static string ReadString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value)
            ? value.GetString()?.Trim() ?? string.Empty
            : string.Empty;
    }

    private static string ExtractJson(string content)
    {
        var trimmed = content.Trim();
        if (trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            var firstBrace = trimmed.IndexOf('{');
            var lastBrace = trimmed.LastIndexOf('}');
            if (firstBrace >= 0 && lastBrace > firstBrace)
            {
                return trimmed.Substring(firstBrace, lastBrace - firstBrace + 1);
            }
        }

        return trimmed;
    }

    private static Season? ParseSeason(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (Enum.TryParse<Season>(value, true, out var exact))
        {
            return exact;
        }

        var normalized = value.Trim().ToLowerInvariant();
        return normalized switch
        {
            var s when s.Contains("prolj") || s.Contains("spring") => Season.Spring,
            var s when s.Contains("ljeto") || s.Contains("summer") => Season.Summer,
            var s when s.Contains("jesen") || s.Contains("autumn") || s.Contains("fall") => Season.Autumn,
            var s when s.Contains("zima") || s.Contains("winter") => Season.Winter,
            _ => null
        };
    }

    private static FishFlesh? ParseFlesh(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (Enum.TryParse<FishFlesh>(value, true, out var exact))
        {
            return exact;
        }

        var normalized = value.Trim().ToLowerInvariant();
        return normalized switch
        {
            var s when s.Contains("bijel") || s.Contains("white") => FishFlesh.White,
            var s when s.Contains("plav") || s.Contains("blue") => FishFlesh.Blue,
            var s when s.Contains("crven") || s.Contains("red") => FishFlesh.Red,
            _ => null
        };
    }

    private string GuessSpecies(string prompt)
    {
        var explicitSpecies = ExtractSpeciesFromPrompt(prompt);
        if (!string.IsNullOrWhiteSpace(explicitSpecies))
        {
            return explicitSpecies;
        }

        var fromKnownFish = repository.Fish.FirstOrDefault(f =>
            prompt.Contains(f.SpeciesName, StringComparison.OrdinalIgnoreCase));

        if (fromKnownFish != null)
        {
            return fromKnownFish.SpeciesName;
        }

        var words = prompt.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (words.Length == 0)
        {
            return "Nova vrsta";
        }

        if (words.Length == 1)
        {
            return Capitalize(words[0]);
        }

        return string.Join(' ', words.Take(2).Select(Capitalize));
    }

    private static string ExtractSpeciesFromPrompt(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            return string.Empty;
        }

        var trimmed = prompt.Trim();

        var quotedMatch = System.Text.RegularExpressions.Regex.Match(trimmed, "[\"'](?<name>[^\"']{2,60})[\"']");
        if (quotedMatch.Success)
        {
            return quotedMatch.Groups["name"].Value.Trim();
        }

        var markers = new[]
        {
            "for ", "fish ", "species ", "vrstu ", "ribu "
        };

        foreach (var marker in markers)
        {
            var index = trimmed.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (index < 0)
            {
                continue;
            }

            var after = trimmed[(index + marker.Length)..].Trim();
            if (string.IsNullOrWhiteSpace(after))
            {
                continue;
            }

            var stopWords = new[] { " with ", " and ", ",", ".", " using ", " u ", " sa " };
            var stopIndex = after.Length;
            foreach (var stopWord in stopWords)
            {
                var matchIndex = after.IndexOf(stopWord, StringComparison.OrdinalIgnoreCase);
                if (matchIndex >= 0)
                {
                    stopIndex = Math.Min(stopIndex, matchIndex);
                }
            }

            var candidate = after[..stopIndex].Trim();
            if (!string.IsNullOrWhiteSpace(candidate))
            {
                return candidate;
            }
        }

        return string.Empty;
    }

    private static string Capitalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        var lower = value.ToLowerInvariant();
        return char.ToUpperInvariant(lower[0]) + lower[1..];
    }

    private readonly record struct SpeciesProfile(
        Season CatchSeason,
        FishFlesh FleshColor,
        string TechniqueName,
        string BaitName,
        FReelType ReelType = FReelType.Spinning,
        FRodAction RodAction = FRodAction.Medium,
        FLineType LineType = FLineType.Nylon);
}
