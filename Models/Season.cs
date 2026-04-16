using System.ComponentModel.DataAnnotations;

namespace FishingBuddy.Models;

public enum Season
{
    [Display(Name = "Proljeće")]
    Spring,
    [Display(Name = "Ljeto")]
    Summer,
    [Display(Name = "Jesen")]
    Autumn,
    [Display(Name = "Zima")]
    Winter
}