using System.ComponentModel.DataAnnotations;

namespace GestApp.Models.Models;

public class LoginRequestDto
{
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public bool Succeeded { get; set; }
    public bool RequiresTwoFactor { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsLockedOut { get; set; }
}

public class TwoFactorLoginRequestDto
{
    [Required(ErrorMessage = "Authenticator code is required.")]
    public string TwoFactorCode { get; set; } = string.Empty;
}