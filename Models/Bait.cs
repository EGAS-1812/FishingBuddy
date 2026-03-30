namespace FishingBuddy.Models;

public class Bait
{
    public int BaitID { get; set; }
    public string BaitName { get; set; } = string.Empty;
    public string BaitType { get; set; } = string.Empty;
    public string PreparationNote { get; set; } = string.Empty;

    public Bait() { }

    public Bait(int baitID, string baitName, string baitType, string preparationNote)
    {
        BaitID = baitID;
        BaitName = baitName ?? string.Empty;
        BaitType = baitType ?? string.Empty;
        PreparationNote = preparationNote ?? string.Empty;
    }
}