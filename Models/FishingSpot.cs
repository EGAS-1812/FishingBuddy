using System.Collections.Generic;
namespace FishingBuddy.Models;

public class FishingSpot
{
    public int SpotID { get; set; }
    public string SpotName { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public bool HasPiers { get; set; }
    public bool BoatAccess { get; set; }
    public List<Fish> MostLikelyCatch { get; set; } = new List<Fish>();

    public FishingSpot() { }

    public FishingSpot(int spotID, string spotName, string region, bool hasPiers, bool boatAccess, IEnumerable<Fish>? mostLikelyCatch = null)
    {
        SpotID = spotID;
        SpotName = spotName ?? string.Empty;
        Region = region ?? string.Empty;
        HasPiers = hasPiers;
        BoatAccess = boatAccess;
        MostLikelyCatch = mostLikelyCatch != null ? new List<Fish>(mostLikelyCatch) : new List<Fish>();
    }
}