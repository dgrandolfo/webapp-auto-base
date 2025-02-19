using System.ComponentModel.DataAnnotations;

namespace GestApp.Application.Models;

public class LoginModel
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public required string Password { get; set; }
}
