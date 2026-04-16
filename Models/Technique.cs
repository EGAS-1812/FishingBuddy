namespace FishingBuddy.Models;

public class Technique
{
    public int TechniqueID { get; set; }
    public string TechniqueName { get; set; } = string.Empty;
    public string PerformanceNote { get; set; } = string.Empty;
    public string TutorialUrl { get; set; } = string.Empty;

    public Technique() { }

    public Technique(int techniqueID, string techniqueName, string performanceNote = "", string tutorialUrl = "")
    {
        TechniqueID = techniqueID;
        TechniqueName = techniqueName ?? string.Empty;
        PerformanceNote = performanceNote ?? string.Empty;
        TutorialUrl = tutorialUrl ?? string.Empty;
    }
}