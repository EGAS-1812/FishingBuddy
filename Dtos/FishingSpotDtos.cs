using System.ComponentModel.DataAnnotations;

namespace FishingBuddy.Dtos;

public class FishingSpotDto
{
    public int SpotID { get; set; }
    public string SpotName { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public bool HasPiers { get; set; }
    public bool BoatAccess { get; set; }
    public List<FishSummaryDto> MostLikelyCatch { get; set; } = new();
}

public class FishingSpotUpsertDto
{
    [Required]
    [MaxLength(120)]
    public string SpotName { get; set; } = string.Empty;

    [Required]
    [MaxLength(120)]
    public string Region { get; set; } = string.Empty;

    public bool HasPiers { get; set; }
    public bool BoatAccess { get; set; }
}
