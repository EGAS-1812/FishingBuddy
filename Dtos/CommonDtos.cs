namespace FishingBuddy.Dtos;

public class TechniqueSummaryDto
{
    public int TechniqueID { get; set; }
    public string TechniqueName { get; set; } = string.Empty;
}

public class BaitSummaryDto
{
    public int BaitID { get; set; }
    public string BaitName { get; set; } = string.Empty;
}

public class FishSummaryDto
{
    public int FishID { get; set; }
    public string SpeciesName { get; set; } = string.Empty;
}

public class UserSummaryDto
{
    public int UserID { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
