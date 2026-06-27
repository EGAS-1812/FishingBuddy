using FishingBuddy.Models;

namespace FishingBuddy.Models.Ai;

public class AiFishDraftResultViewModel
{
    public string SpeciesName { get; set; } = string.Empty;
    public Season? CatchSeason { get; set; }
    public FishFlesh? FleshColor { get; set; }
    public int? FavouriteBaitID { get; set; }
    public int? PreferredMethodID { get; set; }
    public string SuggestedBaitName { get; set; } = string.Empty;
    public string SuggestedTechniqueName { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string Source { get; set; } = "heuristic";
    public string Model { get; set; } = "local";

    // Equipment
    public FReelType? SuggestedReelType { get; set; }
    public int? SuggestedReelSize { get; set; }
    public FRodAction? SuggestedRodAction { get; set; }
    public decimal? SuggestedRodLengthMeters { get; set; }
    public int? SuggestedRodMinWeightGrams { get; set; }
    public int? SuggestedRodMaxWeightGrams { get; set; }
    public FLineType? SuggestedLineType { get; set; }
    public decimal? SuggestedLineThicknessMm { get; set; }

    // Fishing spots
    public IReadOnlyList<string> SuggestedSpotNames { get; set; } = Array.Empty<string>();
}
