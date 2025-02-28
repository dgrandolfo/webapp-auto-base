using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using GestApp.Models.Models;

namespace GestApp.Application.Services.Interfaces;

public interface IAuthenticationService
{
    /// <summary>
    /// Registra un nuovo utente basandosi sui dati forniti.
    /// </summary>
    /// <param name="model">Il modello di creazione dell'utente.</param>
    /// <returns>Un <see cref="IdentityResult"/> che indica se l'operazione è andata a buon fine.</returns>
    Task<IdentityResult> RegisterAsync(UserCreateDto model);
    /// <summary>
    /// Effettua il logout dell'utente corrente.
    /// </summary>
    /// <returns>Un task che rappresenta l'operazione asincrona di logout.</returns>
    Task LogoutAsync();
    /// <summary>
    /// Esegue il login di un utente.
    /// Se l'utente ha il 2FA abilitato, effettua un login parziale per abilitare il flusso a due fattori.
    /// </summary>
    /// <param name="loginRequest">I dati per il login.</param>
    /// <returns>Un <see cref="LoginResponseDto"/> che indica l'esito del login e se è richiesto il 2FA.</returns>
    Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequest);
    /// <summary>
    /// Esegue il login a due fattori (2FA) utilizzando il codice fornito.
    /// </summary>
    /// <param name="model">Il modello contenente il codice di autenticazione a due fattori.</param>
    /// <returns>Un <see cref="LoginResponseDto"/> con l'esito dell'operazione.</returns>
    Task<LoginResponseDto> TwoFactorLoginAsync(TwoFactorLoginRequestDto model);
    /// <summary>
    /// Ottiene i dati di configurazione per l'autenticatore, inclusa la chiave condivisa, l'URI e il QR code.
    /// </summary>
    /// <param name="userPrincipal">Il claims principal dell'utente corrente.</param>
    /// <returns>Un <see cref="AuthenticatorSetupDto"/> contenente i dati per configurare l'autenticatore.</returns>
    Task<AuthenticatorSetupDto> GetAuthenticatorSetupDataAsync(ClaimsPrincipal userPrincipal);
    /// <summary>
    /// Verifica il codice di autenticazione a due fattori e abilita il 2FA per l'utente corrente.
    /// </summary>
    /// <param name="dto">Il DTO contenente il codice di verifica inviato dal client.</param>
    /// <param name="user">Il ClaimsPrincipal dell'utente corrente.</param>
    /// <returns>
    /// Un oggetto <see cref="TwoFactorVerificationResponseDto"/> che indica l'esito della verifica e contiene, se generati, i codici di recupero.
    /// </returns>
    Task<TwoFactorLoginResponseDto> VerifyTwoFactorAsync(string code, ClaimsPrincipal user);
}
