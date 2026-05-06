using System.ComponentModel.DataAnnotations;

namespace FishingBuddy.Models;

public class Bait
{
    [Key]
    public int BaitID { get; set; }

    [Required]
    [MaxLength(100)]
    public string BaitName { get; set; } = string.Empty;

    public BaitType BaitType { get; set; } = BaitType.Live;

    [MaxLength(200)]
    public string PreparationMethod { get; set; } = string.Empty;

    [Range(0, 10_000)]
    public decimal AveragePriceEur { get; set; }

    public virtual ICollection<Fish> PreferredByFish { get; set; } = new HashSet<Fish>();

    public Bait() { }

    public Bait(int baitID, string baitName, BaitType baitType, string preparationMethod, decimal averagePriceEur)
    {
        BaitID = baitID;
        BaitName = baitName ?? string.Empty;
        BaitType = baitType;
        PreparationMethod = preparationMethod ?? string.Empty;
        AveragePriceEur = averagePriceEur;
    }
}