﻿using GestApp.Application.Services.Interfaces;
using GestApp.Infrastructure.Data;
using GestApp.Models.Models;
using Microsoft.AspNetCore.Identity;
using AutoMapper;
using System.Security.Claims;

namespace GestApp.Application.Services;

/// <inheritdoc />
public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;

    /// <summary>
    /// Inizializza una nuova istanza di <see cref="UserService"/>.
    /// </summary>
    /// <param name="userManager">Il gestore degli utenti.</param>
    /// <param name="mapper">Il mapper per convertire tra modelli.</param>
    public UserService(UserManager<ApplicationUser> userManager, IMapper mapper)
    {
        _userManager = userManager;
        _mapper = mapper;
    }

    /// <inheritdoc />
    public async Task<UserResponseDto> GetCurrentUserAsync(ClaimsPrincipal user)
    {
        var currentUser = await _userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return new UserResponseDto { Succeeded = false, Message = "User not found." };
        }

        var roles = await _userManager.GetRolesAsync(currentUser);
        var userDto = _mapper.Map<UserDto>(currentUser);
        userDto.Role = roles.FirstOrDefault();

        return new UserResponseDto { Succeeded = true, User = userDto };
    }

    /// <inheritdoc />
    public async Task<UserResponseDto> GetUserByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return new UserResponseDto { Succeeded = false, Message = "User not found." };
        }

        var roles = await _userManager.GetRolesAsync(user);
        var userData = _mapper.Map<UserDto>(user);
        userData.Role = roles.FirstOrDefault();

        return new UserResponseDto { Succeeded = true, User = userData };
    }

    /// <inheritdoc />
    public async Task<UserResponseDto> UpdateUserAsync(UserUpdateDto updateDto)
    {
        var user = await _userManager.FindByEmailAsync(updateDto.Email);
        if (user == null)
        {
            return new UserResponseDto { Succeeded = false, Message = "User not found." };
        }

        user.Name = updateDto.Name;
        user.Surname = updateDto.Surname;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return new UserResponseDto
            {
                Succeeded = false,
                Message = string.Join(", ", result.Errors.Select(e => e.Description))
            };
        }

        var roles = await _userManager.GetRolesAsync(user);
        var userDto = _mapper.Map<UserDto>(user);
        userDto.Role = roles.FirstOrDefault();

        return new UserResponseDto { Succeeded = true, User = userDto };
    }
}
