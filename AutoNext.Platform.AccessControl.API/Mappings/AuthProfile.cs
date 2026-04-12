using AutoMapper;
using AutoNext.Platform.AccessControl.API.Models.DTOs;
using AutoNext.Platform.AccessControl.API.Models.Entities;

namespace AutoNext.Platform.AccessControl.API.Mappings
{
    public class AuthProfile : Profile
    {
        public AuthProfile()
        {
            // Register to User
            CreateMap<RegisterRequest, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.LastLoginAt, opt => opt.Ignore())
                .ForMember(dest => dest.EmailVerified, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.PhoneVerified, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.UserRoles, opt => opt.Ignore())
                .ForMember(dest => dest.UserOrganizations, opt => opt.Ignore())
                .ForMember(dest => dest.RefreshTokens, opt => opt.Ignore())
                .ForMember(dest => dest.UserSessions, opt => opt.Ignore())
                .ForMember(dest => dest.Metadata, opt => opt.Ignore());

        }
    }
}
