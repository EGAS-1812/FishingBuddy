namespace FishingBuddy.Models;

public class FLine
{
    public FLineType Type { get; set; } = FLineType.Nylon;
    public decimal ThicknessMm { get; set; }

    public FLine() { }

    public FLine(FLineType type, decimal thicknessMm)
    {
        Type = type;
        ThicknessMm = thicknessMm;
    }
}