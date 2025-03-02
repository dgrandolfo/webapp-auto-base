using GestApp.Models.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace GestApp.Application.Services.Interfaces;

/// <summary>
/// Fornisce operazioni per la gestione degli utenti.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Recupera i dati dell'utente corrente basandosi sul ClaimsPrincipal.
    /// </summary>
    /// <param name="user">Il ClaimsPrincipal dell'utente corrente.</param>
    /// <returns>
    /// Un task che restituisce un oggetto <see cref="UserResponseDto"/> contenente i dati dell'utente.
    /// Se l'utente non viene trovato, il risultato avrà <c>Succeeded</c> impostato a false.
    /// </returns>
    Task<UserResponseDto> GetCurrentUserAsync(ClaimsPrincipal user);

    /// <summary>
    /// Recupera i dati dell'utente in base all'email.
    /// </summary>
    /// <param name="email">L'email dell'utente da cercare.</param>
    /// <returns>
    /// Un task che rappresenta l'operazione asincrona. Il risultato contiene un oggetto <see cref="UserResponseDto"/> con i dati dell'utente.
    /// </returns>
    Task<UserResponseDto> GetUserByEmailAsync(string email);

    /// <summary>
    /// Recupera la lista di tutti gli utenti presenti nel database.
    /// </summary>
    /// <returns>
    /// Una <see cref="Task{TResult}"/> che rappresenta l'operazione asincrona e restituisce una lista di <see cref="UserDto"/>.
    /// </returns>
    Task<List<UserDto>> GetAllUsersAsync();

    /// <summary>
    /// Aggiorna il nome e il cognome dell'utente.
    /// </summary>
    /// <param name="updateDto">I dati per l'aggiornamento dell'utente, contenenti email, name e surname.</param>
    /// <returns>
    /// Un task che rappresenta l'operazione asincrona. Il risultato contiene un oggetto <see cref="UserResponseDto"/> con i dati aggiornati dell'utente.
    /// </returns>
    Task<UserResponseDto> UpdateUserAsync(UserUpdateDto updateDto);

    /// <summary>
    /// Elimina l'utente identificato dall'email specificata.
    /// </summary>
    /// <param name="email">L'email dell'utente da eliminare.</param>
    /// <returns>
    /// Un <see cref="Task{IdentityResult}"/> che indica l'esito dell'operazione.
    /// </returns>
    Task<IdentityResult> DeleteUserAsync(string email);
}
