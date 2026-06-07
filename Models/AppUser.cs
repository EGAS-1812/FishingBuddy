using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace FishingBuddy.Models;

public class AppUser : IdentityUser
{
    [StringLength(11, MinimumLength = 11)]
    [RegularExpression("^[0-9]*$", ErrorMessage = "OIB smije sadržavati samo brojeve.")]
    public string? OIB { get; set; }

    [StringLength(200)]
    public string? Address { get; set; }
}
