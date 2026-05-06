using Microsoft.EntityFrameworkCore;

namespace FishingBuddy.Models;

[Owned]
public class FReel
{
    public int Size { get; set; }
    public FReelType Type { get; set; } = FReelType.Spinning;

    public FReel() { }

    public FReel(int size, FReelType type)
    {
        Size = size;
        Type = type;
    }
}