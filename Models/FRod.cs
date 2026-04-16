namespace FishingBuddy.Models;

public class FRod
{
    public decimal LengthMeters { get; set; }
    public FRodAction Action { get; set; } = FRodAction.Medium;
    public int MinWeightRatingGrams { get; set; }
    public int MaxWeightRatingGrams { get; set; }

    public FRod() { }

    public FRod(decimal lengthMeters, FRodAction action, int minWeightRatingGrams, int maxWeightRatingGrams)
    {
        LengthMeters = lengthMeters;
        Action = action;
        MinWeightRatingGrams = minWeightRatingGrams;
        MaxWeightRatingGrams = maxWeightRatingGrams;
    }
}