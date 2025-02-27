using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using GestApp.Infrastructure.Data;
using GestApp.Models.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;

namespace GestApp.Controllers;

//[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;

    public UserController(UserManager<ApplicationUser> userManager,
        IMapper mapper)
    {
        _userManager = userManager;
        _mapper = mapper;
    }

    [HttpGet("{email}")]
    public async Task<IActionResult> GetUserByEmail(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return NotFound(new UserResponseDto { Succeeded = false, Message = "User not found." });

        // Recupera i ruoli associati all'utente
        var roles = await _userManager.GetRolesAsync(user);

        // Prepara i dati da restituire
        UserDto userData = _mapper.Map<UserDto>(user);
        userData.Role = roles.FirstOrDefault();

        return Ok(new UserResponseDto { Succeeded = true, User = userData });
    }

    [HttpPost("update")]
    public async Task<IActionResult> UpdateUser([FromBody] UserUpdateDto updateDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userManager.FindByEmailAsync(updateDto.Email);
        if (user == null)
            return NotFound(new UserResponseDto { Succeeded = false, Message = "User not found." });

        // Aggiorna i campi desiderati
        user.Name = updateDto.Name;
        user.Surname = updateDto.Surname;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return BadRequest(new UserResponseDto { Succeeded = false, Message = string.Join(", ", result.Errors.Select(e => e.Description)) });
        }

        // Recupera i ruoli associati all'utente
        var roles = await _userManager.GetRolesAsync(user);

        var userDto = _mapper.Map<UserDto>(user);
        userDto.Role = roles.FirstOrDefault();

        return Ok(new UserResponseDto { Succeeded = true, User = userDto });
    }
}
