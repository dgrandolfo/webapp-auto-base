using GestApp.Models.Models;
using GestApp.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text.Encodings.Web;
using System.Text;
using QRCoder;
using Microsoft.Extensions.Options;
using GestApp.Application.Configurations;
using Microsoft.AspNetCore.Authorization;

namespace GestApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UrlEncoder _urlEncoder;
    private readonly IOptions<Settings> _options;
    private readonly IConfiguration _configuration;

    public AccountController(UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        SignInManager<ApplicationUser> signInManager,
        UrlEncoder urlEncoder,
        IOptions<Settings> options,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
        _urlEncoder = urlEncoder;
        _options = options;
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

    //[Authorize]
    [HttpGet("2fa/setup")]
    public async Task<IActionResult> GetAuthenticatorSetupData()
    {
        // Recupera l'utente corrente
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("User not found.");

        // Ottiene la chiave non formattata per l'autenticatore
        var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(unformattedKey))
        {
            await _userManager.ResetAuthenticatorKeyAsync(user);
            unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
        }

        // Format key per renderla più leggibile
        var sharedKey = FormatKey(unformattedKey);

        // Recupera l'email
        var email = await _userManager.GetEmailAsync(user);

        // Genera l'URI per il QR code
        var authenticatorUri = GenerateQrCodeUri(email ?? "user@example.com", unformattedKey);

        // Genera il QR code (in formato data:image/png;base64)
        var qrCodeImage = GenerateQrCode(authenticatorUri);

        var dto = new AuthenticatorSetupDto
        {
            SharedKey = sharedKey,
            AuthenticatorUri = authenticatorUri,
            QrCodeImage = qrCodeImage
        };

        return Ok(dto);
    }

    private string FormatKey(string unformattedKey)
    {
        var result = new StringBuilder();
        int currentPosition = 0;
        while (currentPosition + 4 < unformattedKey.Length)
        {
            result.Append(unformattedKey.AsSpan(currentPosition, 4)).Append(' ');
            currentPosition += 4;
        }
        if (currentPosition < unformattedKey.Length)
        {
            result.Append(unformattedKey.AsSpan(currentPosition));
        }
        return result.ToString().ToLowerInvariant();
    }

    private string GenerateQrCodeUri(string email, string unformattedKey)
    {
        // Recupera il nome dell'app da appsettings.json
        string platformName = _options.Value.AppName;

        return string.Format(
            CultureInfo.InvariantCulture,
            "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&algorithm=SHA1&digits=6&period=30",
            _urlEncoder.Encode(platformName),
            _urlEncoder.Encode(email),
            unformattedKey);
    }

    private string GenerateQrCode(string uri)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(uri, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        byte[] qrCodeBytes = qrCode.GetGraphic(20);
        return $"data:image/png;base64,{Convert.ToBase64String(qrCodeBytes)}";
    }
}
