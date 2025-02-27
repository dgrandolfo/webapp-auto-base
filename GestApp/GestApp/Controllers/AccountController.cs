using GestApp.Application.DTOs;
using GestApp.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GestApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;

    public AccountController(UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserCreateDto model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            return Ok(new { Message = "Utente registrato con successo" });
        }

        return BadRequest(result.Errors);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _userManager.FindByEmailAsync(loginRequest.Email);
        if (user == null)
        {
            return Unauthorized(new LoginResponseDto { Succeeded = false, Message = "Invalid email or password." });
        }

        // Controlla direttamente la proprietà TwoFactorEnabled
        if (user.TwoFactorEnabled)
        {
            // Esegui il login parziale per abilitare il flusso 2FA.
            // Questo serve per impostare il contesto del 2FA.
            await _signInManager.PasswordSignInAsync(user, loginRequest.Password, false, lockoutOnFailure: false);
            return Ok(new LoginResponseDto { Succeeded = true, RequiresTwoFactor = true });
        }

        // Login normale se il 2FA non è abilitato
        var result = await _signInManager.CheckPasswordSignInAsync(user, loginRequest.Password, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            // Esegue il sign in e imposta il cookie di autenticazione
            await _signInManager.SignInAsync(user, isPersistent: false);
            return Ok(new LoginResponseDto { Succeeded = true });
        }
        else
        {
            return Unauthorized(new LoginResponseDto { Succeeded = false, Message = "Invalid email or password." });
        }
    }


    [HttpPost("2fa")]
    public async Task<IActionResult> TwoFactorLogin([FromBody] TwoFactorLoginRequestDto model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Recupera l'utente in fase di 2FA
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            return Unauthorized(new { message = "Unable to load two-factor authentication user." });
        }

        // Normalizza il codice (rimuove spazi e trattini)
        var code = model.TwoFactorCode?.Replace(" ", string.Empty).Replace("-", string.Empty);

        var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(code!, false, false);
        if (result.Succeeded)
        {
            return Ok(new { message = "Two-factor authentication successful." });
        }
        if (result.IsLockedOut)
        {
            return StatusCode(StatusCodes.Status423Locked, new { message = "User account locked out." });
        }
        return Unauthorized(new { message = "Invalid authenticator code." });
    }
}