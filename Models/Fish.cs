namespace FishingBuddy.Models;

public class Fish
{
    public int FishID { get; set; }
    public string SpeciesName { get; set; } = string.Empty;
    public Season CatchSeason { get; set; }
    public int FavouriteBaitID { get; set; }
    public FishFlesh FleshColor { get; set; }
    public Technique PreferredMethod { get; set; } = new Technique();
    public Equipment Equipment { get; set; } = new Equipment();

    public Fish() { }

    public Fish(int fishID, string speciesName, Season catchSeason, int favouriteBaitID, FishFlesh fleshColor, Technique? preferredMethod = null, Equipment? equipment = null)
    {
        FishID = fishID;
        SpeciesName = speciesName ?? string.Empty;
        CatchSeason = catchSeason;
        FavouriteBaitID = favouriteBaitID;
        FleshColor = fleshColor;
        PreferredMethod = preferredMethod ?? new Technique();
        Equipment = equipment ?? new Equipment();
    }
}