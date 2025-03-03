using Microsoft.AspNetCore.Mvc;
using GestApp.Application.Services.Interfaces;
using GestApp.Models.Models;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;

namespace GestApp.Controllers;

/// <summary>
/// Controller per le operazioni relative agli utenti.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    /// <summary>
    /// Inizializza una nuova istanza di <see cref="UserController"/>.
    /// </summary>
    /// <param name="userService">Il servizio per la gestione degli utenti.</param>
    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Crea un nuovo utente utilizzando i dati forniti.
    /// </summary>
    /// <param name="userToCreate">I dati per la creazione dell'utente.</param>
    /// <returns>Un oggetto <see cref="UserResponseDto"/> che indica l'esito dell'operazione.</returns>
    [HttpPost("create")]
    public async Task<IActionResult> CreateUser([FromBody] UserCreateDto userToCreate)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _userService.CreateUserAsync(userToCreate);
        if (result.Succeeded)
        {
            return Ok(new UserResponseDto { Succeeded = true });
        }
        else
        {
            return BadRequest(new UserResponseDto
            {
                Succeeded = false,
                Message = string.Join(", ", result.Errors.Select(e => e.Description).FirstOrDefault())
            });
        }
    }

    /// <summary>
    /// Recupera i dati dell'utente corrente.
    /// </summary>
    /// <returns>
    /// Un oggetto <see cref="UserResponseDto"/> contenente i dati dell'utente corrente.
    /// Se l'utente non è autenticato o non viene trovato, restituisce Unauthorized.
    /// </returns>
    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var result = await _userService.GetCurrentUserAsync(User);
        if (!result.Succeeded)
            return Unauthorized(result);
        return Ok(result);
    }

    /// <summary>
    /// Recupera i dati dell'utente in base all'email.
    /// </summary>
    /// <param name="email">L'email dell'utente da cercare.</param>
    /// <returns>Un oggetto <see cref="UserResponseDto"/> con i dati dell'utente.</returns>
    [HttpGet("{email}")]
    public async Task<IActionResult> GetUserByEmail(string email)
    {
        var response = await _userService.GetUserByEmailAsync(email);
        if (!response.Succeeded)
            return NotFound(response);

        return Ok(response);
    }

    /// <summary>
    /// Recupera la lista di tutti gli utenti presenti nel database.
    /// </summary>
    /// <returns>Una lista di <see cref="UserDto"/>.</returns>
    [Authorize(Roles = "Admin")]
    [HttpGet("all")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    /// <summary>
    /// Aggiorna il nome e il cognome dell'utente.
    /// </summary>
    /// <param name="updateDto">I dati per l'aggiornamento dell'utente.</param>
    /// <returns>Un oggetto <see cref="UserResponseDto"/> con i dati aggiornati dell'utente.</returns>
    [HttpPatch("update")]
    public async Task<IActionResult> UpdateUser([FromBody] UserUpdateDto updateDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = await _userService.UpdateUserAsync(updateDto);
        if (!response.Succeeded)
            return BadRequest(response);

        return Ok(response);
    }

    /// <summary>
    /// Elimina l'utente identificato dall'email specificata.
    /// </summary>
    /// <param name="dto">Il DTO contenente l'email dell'utente da eliminare.</param>
    /// <returns>Un oggetto <see cref="UserResponseDto"/> che indica l'esito dell'operazione.</returns>
    [Authorize(Roles = "Admin")]
    [HttpDelete("delete/{email}")]
    public async Task<IActionResult> DeleteUser(string email)
    {
        var result = await _userService.DeleteUserAsync(email);
        if (!result.Succeeded)
        {
            return BadRequest(new UserResponseDto { Succeeded = false, Message = string.Join(", ", result.Errors.Select(e => e.Description)) });
        }
        return Ok(new UserResponseDto { Succeeded = true });
    }
}
