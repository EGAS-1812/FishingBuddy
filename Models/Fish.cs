using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FishingBuddy.Models;

public class Fish
{
    [Key]
    public int FishID { get; set; }

    [Required]
    [MaxLength(120)]
    public string SpeciesName { get; set; } = string.Empty;

    public Season CatchSeason { get; set; }

    public int FavouriteBaitID { get; set; }

    public FishFlesh FleshColor { get; set; }

    public int PreferredMethodID { get; set; }

    [ForeignKey(nameof(FavouriteBaitID))]
    public virtual Bait? FavouriteBait { get; set; }

    [ForeignKey(nameof(PreferredMethodID))]
    public virtual Technique PreferredMethod { get; set; } = null!;

    public Equipment Equipment { get; set; } = new Equipment();

    public virtual ICollection<CatchRecord> CatchRecords { get; set; } = new HashSet<CatchRecord>();
    public virtual ICollection<User> FavoritedByUsers { get; set; } = new HashSet<User>();
    public virtual ICollection<FishingSpot> PossibleSpots { get; set; } = new HashSet<FishingSpot>();

    public Fish() { }

    public Fish(int fishID, string speciesName, Season catchSeason, int favouriteBaitID, FishFlesh fleshColor, Technique? preferredMethod = null, Equipment? equipment = null)
    {
        FishID = fishID;
        SpeciesName = speciesName ?? string.Empty;
        CatchSeason = catchSeason;
        FavouriteBaitID = favouriteBaitID;
        FleshColor = fleshColor;
        PreferredMethod = preferredMethod ?? new Technique();
        PreferredMethodID = PreferredMethod.TechniqueID;
        Equipment = equipment ?? new Equipment();
    }
}