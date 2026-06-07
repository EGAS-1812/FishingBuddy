using System.ComponentModel.DataAnnotations;

namespace FishingBuddy.Dtos;

public class TechniqueDto
{
    public int TechniqueID { get; set; }
    public string TechniqueName { get; set; } = string.Empty;
    public string PerformanceNote { get; set; } = string.Empty;
    public string TutorialUrl { get; set; } = string.Empty;
}

public class TechniqueUpsertDto
{
    [Required]
    [MaxLength(120)]
    public string TechniqueName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string PerformanceNote { get; set; } = string.Empty;

    [MaxLength(300)]
    [Url]
    public string TutorialUrl { get; set; } = string.Empty;
}
