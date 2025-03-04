using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using QRCoder;
using GestApp.Application.Services.Interfaces;
using GestApp.Infrastructure.Data;
using GestApp.Application.Configurations;
using GestApp.Models.Models;
using System.Security.Claims;

namespace GestApp.Application.Services;

/// <summary>
/// Fornisce metodi per la registrazione, il login, il flusso a due fattori e la configurazione dell'autenticatore.
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UrlEncoder _urlEncoder;
    private readonly IOptions<Settings> _options;

    /// <summary>
    /// Inizializza una nuova istanza di <see cref="AuthenticationService"/>.
    /// </summary>
    /// <param name="userManager">User manager per <see cref="ApplicationUser"/>.</param>
    /// <param name="signInManager">Sign-in manager per <see cref="ApplicationUser"/>.</param>
    /// <param name="urlEncoder">Servizio per l'encoding degli URL.</param>
    /// <param name="options">Opzioni applicative contenenti le impostazioni (ad esempio, AppName).</param>
    public AuthenticationService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        UrlEncoder urlEncoder,
        IOptions<Settings> options)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _urlEncoder = urlEncoder;
        _options = options;
    }

    /// <inheritdoc />
    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }

    /// <inheritdoc />
    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequest)
    {
        var user = await _userManager.FindByEmailAsync(loginRequest.Email);
        if (user == null)
            return new LoginResponseDto { Succeeded = false, Message = "Invalid email or password." };

        if (user.TwoFactorEnabled)
        {
            // Esegue un login parziale per abilitare il flusso 2FA.
            await _signInManager.PasswordSignInAsync(user, loginRequest.Password, false, lockoutOnFailure: false);
            return new LoginResponseDto { Succeeded = true, RequiresTwoFactor = true };
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, loginRequest.Password, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            await _signInManager.SignInAsync(user, isPersistent: false);
            return new LoginResponseDto { Succeeded = true };
        }
        else
        {
            return new LoginResponseDto { Succeeded = false, Message = "Invalid email or password." };
        }
    }

    /// <inheritdoc />
    public async Task<LoginResponseDto> TwoFactorLoginAsync(TwoFactorLoginRequestDto model)
    {
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
            return new LoginResponseDto { Succeeded = false, Message = "Unable to load two-factor authentication user." };

        var code = model.TwoFactorCode?.Replace(" ", string.Empty).Replace("-", string.Empty);
        var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(code!, false, false);
        if (result.Succeeded)
            return new LoginResponseDto { Succeeded = true, Message = "Two-factor authentication successful." };

        if (result.IsLockedOut)
            return new LoginResponseDto { Succeeded = false, Message = "User account locked out.", IsLockedOut = true };

        return new LoginResponseDto { Succeeded = false, Message = "Invalid authenticator code." };
    }

    /// <inheritdoc />
    public async Task<AuthenticatorSetupDto> GetAuthenticatorSetupDataAsync(ClaimsPrincipal userPrincipal)
    {
        var user = await _userManager.GetUserAsync(userPrincipal);
        if (user == null)
            throw new Exception("User not found.");

        var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(unformattedKey))
        {
            await _userManager.ResetAuthenticatorKeyAsync(user);
            unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
        }

        var sharedKey = FormatKey(unformattedKey!);
        var email = await _userManager.GetEmailAsync(user);
        var authenticatorUri = GenerateQrCodeUri(email ?? "user@example.com", unformattedKey!);
        var qrCodeImage = GenerateQrCode(authenticatorUri);

        return new AuthenticatorSetupDto
        {
            SharedKey = sharedKey,
            AuthenticatorUri = authenticatorUri,
            QrCodeImage = qrCodeImage
        };
    }

    /// <inheritdoc />
    public async Task<TwoFactorLoginResponseDto> VerifyTwoFactorAsync(string code, ClaimsPrincipal user)
    {
        var currentUser = await _userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return new TwoFactorLoginResponseDto
            {
                Succeeded = false,
                Message = "User not found."
            };
        }

        // Rimuove spazi e trattini dal codice inserito
        var verificationCode = code.Replace(" ", string.Empty).Replace("-", string.Empty);
        var isValid = await _userManager.VerifyTwoFactorTokenAsync(
            currentUser,
            _userManager.Options.Tokens.AuthenticatorTokenProvider,
            verificationCode);

        if (!isValid)
        {
            return new TwoFactorLoginResponseDto
            {
                Succeeded = false,
                Message = "Error: Verification code is invalid."
            };
        }

        await _userManager.SetTwoFactorEnabledAsync(currentUser, true);

        string message = "Your authenticator app has been verified.";
        IEnumerable<string>? recoveryCodes = null;

        if (await _userManager.CountRecoveryCodesAsync(currentUser) == 0)
        {
            recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(currentUser, 10);
            message = "2FA activated!";
        }
        else
        {
            message = "2FA successfully activated!";
        }

        return new TwoFactorLoginResponseDto
        {
            Succeeded = true,
            Message = message,
            RecoveryCodes = recoveryCodes
        };
    }

    /// <inheritdoc />
    public async Task<AuthenticatorSetupDto> ResetAuthenticatorAsync(ClaimsPrincipal user)
    {
        var currentUser = await _userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            throw new Exception("User not found.");
        }

        // Reset della chiave dell'autenticatore
        await _userManager.ResetAuthenticatorKeyAsync(currentUser);

        // Recupera la nuova chiave non formattata
        var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(currentUser);
        if (string.IsNullOrEmpty(unformattedKey))
        {
            throw new Exception("Unable to retrieve new authenticator key.");
        }

        // Format key per renderla più leggibile
        var sharedKey = FormatKey(unformattedKey);

        // Recupera l'email per generare l'URI
        var email = await _userManager.GetEmailAsync(currentUser);
        var authenticatorUri = GenerateQrCodeUri(email ?? "user@example.com", unformattedKey);

        // Genera il QR code (in formato data:image/png;base64)
        var qrCodeImage = GenerateQrCode(authenticatorUri);

        return new AuthenticatorSetupDto
        {
            SharedKey = sharedKey,
            AuthenticatorUri = authenticatorUri,
            QrCodeImage = qrCodeImage
        };
    }

    #region Private methods
    /// <summary>
    /// Formatta una chiave non formattata per renderla più leggibile.
    /// </summary>
    /// <param name="unformattedKey">La chiave non formattata.</param>
    /// <returns>La chiave formattata.</returns>
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
            result.Append(unformattedKey.AsSpan(currentPosition));

        return result.ToString().ToLowerInvariant();
    }

    /// <summary>
    /// Genera l'URI da utilizzare per il QR code dell'autenticatore.
    /// </summary>
    /// <param name="email">L'email dell'utente.</param>
    /// <param name="unformattedKey">La chiave non formattata.</param>
    /// <returns>L'URI formattato.</returns>
    private string GenerateQrCodeUri(string email, string unformattedKey)
    {
        string platformName = _options.Value.AppName;
        return string.Format(
            CultureInfo.InvariantCulture,
            "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&algorithm=SHA1&digits=6&period=30",
            _urlEncoder.Encode(platformName),
            _urlEncoder.Encode(email),
            unformattedKey);
    }

    /// <summary>
    /// Genera un'immagine QR code codificata in Base64 da un URI.
    /// </summary>
    /// <param name="uri">L'URI da codificare.</param>
    /// <returns>Una stringa contenente il QR code in formato data:image/png;base64.</returns>
    private static string GenerateQrCode(string uri)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(uri, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        byte[] qrCodeBytes = qrCode.GetGraphic(20);
        return $"data:image/png;base64,{Convert.ToBase64String(qrCodeBytes)}";
    }
    #endregion
}
