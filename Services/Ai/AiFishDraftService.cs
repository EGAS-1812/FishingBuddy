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
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return BuildHeuristicDraft(normalizedPrompt);
        }

        try
        {
            return await BuildModelDraftAsync(normalizedPrompt, apiKey, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "OpenAI draft request failed. Falling back to local heuristic parser.");
            var fallback = BuildHeuristicDraft(normalizedPrompt);
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
            result.SpeciesName = BuildHeuristicDraft(prompt).SpeciesName;
        }

        return result;
    }

    private AiFishDraftResultViewModel BuildHeuristicDraft(string prompt)
    {
        var result = new AiFishDraftResultViewModel
        {
            SpeciesName = GuessSpecies(prompt),
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

        if (string.IsNullOrWhiteSpace(result.Notes))
        {
            result.Notes = "Draft generated from local parser. Review values before saving.";
        }

        return result;
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

    private static string Capitalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        var lower = value.ToLowerInvariant();
        return char.ToUpperInvariant(lower[0]) + lower[1..];
    }
}
