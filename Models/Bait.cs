namespace FishingBuddy.Models;

public class Bait
{
    public int BaitID { get; set; }
    public string BaitName { get; set; } = string.Empty;
    public BaitType BaitType { get; set; } = BaitType.Live;
    public string PreparationMethod { get; set; } = string.Empty;
    public decimal AveragePriceEur { get; set; }

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