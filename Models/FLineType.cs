using System.ComponentModel.DataAnnotations;

namespace FishingBuddy.Models;

public enum FLineType
{
    [Display(Name = "Nylon")]
    Nylon,
    [Display(Name = "Braided")]
    Braided
}