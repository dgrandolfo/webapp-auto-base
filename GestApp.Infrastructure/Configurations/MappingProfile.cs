using AutoMapper;
using GestApp.Application.DTOs;
using GestApp.Infrastructure.Data;

namespace GestApp.Infrastructure.Configurations;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Mappatura da UserDto a ApplicationUser
        CreateMap<UserDto, ApplicationUser>();

        // Mappatura da ApplicationUser a UserDto
        CreateMap<ApplicationUser, UserDto>();

        // Mappatura da ApplicationUser a UserCreateDto
        CreateMap<ApplicationUser, UserCreateDto>()
            .ForMember(dest => dest.ConfirmPassword, opt => opt.Ignore()); // Ignora ConfirmPassword

        // Mappatura da UserCreateDto a ApplicationUser
        CreateMap<UserCreateDto, ApplicationUser>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email)) // Mappa Email su UserName
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow)) // Imposta CreatedAt
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow)); // Imposta UpdatedAt

        //// Mappatura da ApplicationUser a UserUpdateDto
        //CreateMap<ApplicationUser, UserUpdateDto>();

        //// Mappatura da UserUpdateDto a ApplicationUser
        //CreateMap<UserUpdateDto, ApplicationUser>();
    }
}