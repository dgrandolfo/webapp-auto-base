using GestApp.Application.Services.Interfaces;
using GestApp.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestApp.Controllers
{
    /// <summary>
    /// Controller per gestire le operazioni di autenticazione.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authService;

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="AuthenticationController"/>.
        /// </summary>
        /// <param name="authService">Il servizio di autenticazione.</param>
        public AuthenticationController(IAuthenticationService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Registra un nuovo utente.
        /// </summary>
        /// <param name="model">I dati necessari per la registrazione dell'utente.</param>
        /// <returns>Un risultato dell'operazione di registrazione.</returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserCreateDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterAsync(model);
            if (result.Succeeded)
                return Ok(new { Message = "Utente registrato con successo" });
            return BadRequest(result.Errors);
        }

        /// <summary>
        /// Effettua il login di un utente.
        /// </summary>
        /// <param name="loginRequest">I dati per il login.</param>
        /// <returns>Un risultato che indica l'esito del login e se è richiesto il 2FA.</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.LoginAsync(loginRequest);
            if (result.Succeeded)
                return Ok(result);
            return Unauthorized(result);
        }

        /// <summary>
        /// Esegue il login a due fattori.
        /// </summary>
        /// <param name="model">I dati per il login a due fattori.</param>
        /// <returns>Un risultato che indica l'esito dell'operazione di 2FA.</returns>
        [HttpPost("2fa")]
        public async Task<IActionResult> TwoFactorLogin([FromBody] TwoFactorLoginRequestDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.TwoFactorLoginAsync(model);
            if (result.Succeeded)
                return Ok(result);
            if (result.IsLockedOut)
                return StatusCode(StatusCodes.Status423Locked, result);
            return Unauthorized(result);
        }

        /// <summary>
        /// Ottiene i dati per la configurazione dell'autenticatore per l'utente corrente.
        /// </summary>
        /// <returns>Un <see cref="AuthenticatorSetupDto"/> contenente shared key, URI e QR code.</returns>
        [Authorize]
        [HttpGet("2fa/setup")]
        public async Task<IActionResult> GetAuthenticatorSetupData()
        {
            try
            {
                var dto = await _authService.GetAuthenticatorSetupDataAsync(User);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }
    }
}
