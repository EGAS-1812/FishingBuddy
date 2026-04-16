using System.ComponentModel.DataAnnotations;

namespace FishingBuddy.Models;

public enum FishFlesh
{
    [Display(Name = "Plavo")]
    Blue,
    [Display(Name = "Crveno")]
    Red,
    [Display(Name = "Bijelo")]
    White
}