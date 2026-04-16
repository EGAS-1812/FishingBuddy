using System.ComponentModel.DataAnnotations;

namespace FishingBuddy.Models;

public enum FRodAction
{
    [Display(Name = "Light")]
    Light,
    [Display(Name = "Medium")]
    Medium,
    [Display(Name = "Medium-Light")]
    MediumLight,
    [Display(Name = "Medium-Heavy")]
    MediumHeavy,
    [Display(Name = "Heavy")]
    Heavy
}