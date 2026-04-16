using System.ComponentModel.DataAnnotations;

namespace FishingBuddy.Models;

public enum BaitType
{
    [Display(Name = "Živi")]
    Live,
    [Display(Name = "Mrtvi")]
    Dead,
    [Display(Name = "Umjetni")]
    Artificial
}