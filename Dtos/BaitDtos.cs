using FishingBuddy.Models;
using System.ComponentModel.DataAnnotations;

namespace FishingBuddy.Dtos;

public class BaitDto
{
    public int BaitID { get; set; }
    public string BaitName { get; set; } = string.Empty;
    public BaitType BaitType { get; set; }
    public string PreparationMethod { get; set; } = string.Empty;
    public decimal AveragePriceEur { get; set; }
}

public class BaitUpsertDto
{
    [Required]
    [MaxLength(100)]
    public string BaitName { get; set; } = string.Empty;

    public BaitType BaitType { get; set; } = BaitType.Live;

    [MaxLength(200)]
    public string PreparationMethod { get; set; } = string.Empty;

    [Range(0, 10_000)]
    public decimal AveragePriceEur { get; set; }
}
