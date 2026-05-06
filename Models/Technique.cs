using System.ComponentModel.DataAnnotations;

namespace FishingBuddy.Models;

public class Technique
{
    [Key]
    public int TechniqueID { get; set; }

    [Required]
    [MaxLength(120)]
    public string TechniqueName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string PerformanceNote { get; set; } = string.Empty;

    [MaxLength(300)]
    [Url]
    public string TutorialUrl { get; set; } = string.Empty;

    public virtual ICollection<Fish> FishUsingTechnique { get; set; } = new HashSet<Fish>();

    public Technique() { }

    public Technique(int techniqueID, string techniqueName, string performanceNote = "", string tutorialUrl = "")
    {
        TechniqueID = techniqueID;
        TechniqueName = techniqueName ?? string.Empty;
        PerformanceNote = performanceNote ?? string.Empty;
        TutorialUrl = tutorialUrl ?? string.Empty;
    }
}