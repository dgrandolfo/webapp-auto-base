using System.ComponentModel.DataAnnotations;

namespace GestApp.Application.DTOs;

public class UserDto
{
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public string FiscalCode { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Role { get; set; }

    public DateTime? CreatedAt { get; set; }
}

public class UserCreateDto
{
    [Required(ErrorMessage = "Name is required.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Surname is required.")]
    public string Surname { get; set; } = string.Empty;

    [Required(ErrorMessage = "Fiscal Code is required.")]
    [RegularExpression(@"^[A-Z]{6}[0-9]{2}[A-Z][0-9]{2}[A-Z][0-9]{3}[A-Z]$",
                       ErrorMessage = "Invalid Italian Fiscal Code.")]
    public string FiscalCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirm Password is required.")]
    [Compare(nameof(Password), ErrorMessage = "Password and Confirm Password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role is required.")]
    public string Role { get; set; } = string.Empty;
}

public class UserLoginDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password, ErrorMessage = "Password too weak")]
    public string Password { get; set; } = string.Empty;
}