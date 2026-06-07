using FishingBuddy.Models;
using System.ComponentModel.DataAnnotations;

namespace FishingBuddy.Dtos;

public class EquipmentDto
{
    public int ReelSize { get; set; }
    public FReelType ReelType { get; set; }
    public decimal RodLengthMeters { get; set; }
    public FRodAction RodAction { get; set; }
    public int RodMinWeightGrams { get; set; }
    public int RodMaxWeightGrams { get; set; }
    public FLineType LineType { get; set; }
    public decimal LineThicknessMm { get; set; }
}

public class FishDto
{
    public int FishID { get; set; }
    public string SpeciesName { get; set; } = string.Empty;
    public Season CatchSeason { get; set; }
    public FishFlesh FleshColor { get; set; }
    public BaitSummaryDto? FavouriteBait { get; set; }
    public TechniqueSummaryDto? PreferredMethod { get; set; }
    public EquipmentDto Equipment { get; set; } = new EquipmentDto();
}

public class FishUpsertDto
{
    [Required]
    [MaxLength(120)]
    public string SpeciesName { get; set; } = string.Empty;

    public Season CatchSeason { get; set; }
    public FishFlesh FleshColor { get; set; }

    [Range(1, int.MaxValue)]
    public int FavouriteBaitID { get; set; }

    [Range(1, int.MaxValue)]
    public int PreferredMethodID { get; set; }

    public EquipmentDto Equipment { get; set; } = new EquipmentDto();
}
