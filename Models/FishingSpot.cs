using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FishingBuddy.Models;

public class FishingSpot
{
    [Key]
    public int SpotID { get; set; }

    [Required]
    [MaxLength(120)]
    public string SpotName { get; set; } = string.Empty;

    [Required]
    [MaxLength(120)]
    public string Region { get; set; } = string.Empty;

    public bool HasPiers { get; set; }
    public bool BoatAccess { get; set; }

    public virtual ICollection<Fish> MostLikelyCatch { get; set; } = new HashSet<Fish>();

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