namespace FishingBuddy.Models;

public class Technique
{
    public int TechniqueID { get; set; }
    public string TechniqueName { get; set; } = string.Empty;

    public Technique() { }

    public Technique(int techniqueID, string techniqueName)
    {
        TechniqueID = techniqueID;
        TechniqueName = techniqueName ?? string.Empty;
    }
}