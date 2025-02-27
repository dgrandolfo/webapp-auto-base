using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace GestApp.Infrastructure.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    [PersonalData]
    public string? Name { get; set; }

    [PersonalData]
    public string? Surname { get; set; }

    public string? FullName => string.IsNullOrWhiteSpace(Name) && string.IsNullOrWhiteSpace(Surname) ? UserName : $"{Name} {Surname}".Trim();

    [Required]
    [StringLength(16)]
    public string FiscalCode { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

}

