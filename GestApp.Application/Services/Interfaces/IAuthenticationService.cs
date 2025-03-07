﻿using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using GestApp.Models.Models;

namespace GestApp.Application.Services.Interfaces;

public interface IAuthenticationService
{
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
    /// <summary>
    /// Resetta la chiave dell'autenticatore per l'utente corrente e restituisce i nuovi dati di configurazione.
    /// </summary>
    /// <param name="user">Il ClaimsPrincipal dell'utente corrente.</param>
    /// <returns>
    /// Un task che restituisce un oggetto <see cref="AuthenticatorSetupDto"/> contenente la nuova chiave formattata, l'URI per il QR code e l'immagine del QR code.
    /// </returns>
    Task<AuthenticatorSetupDto> ResetAuthenticatorAsync(ClaimsPrincipal user);
}
