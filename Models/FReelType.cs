using System.ComponentModel.DataAnnotations;

namespace FishingBuddy.Models;

public enum FReelType
{
    [Display(Name = "Baitcast")]
    Baitcast,
    [Display(Name = "Spinning")]
    Spinning,
    [Display(Name = "Surf")]
    Surf,
    [Display(Name = "Troll")]
    Troll,
    [Display(Name = "Fly")]
    Fly
}