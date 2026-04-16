namespace FishingBuddy.Models;

public class Equipment
{
    public FReel FReel { get; set; } = new FReel();
    public FRod FRod { get; set; } = new FRod();
    public FLine FLine { get; set; } = new FLine();

    public Equipment() { }

    public Equipment(FReel fReel, FRod fRod, FLine fLine)
    {
        FReel = fReel ?? new FReel();
        FRod = fRod ?? new FRod();
        FLine = fLine ?? new FLine();
    }
}