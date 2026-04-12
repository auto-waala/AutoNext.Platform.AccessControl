using AutoMapper;
using AutoNext.Platform.AccessControl.API.Models.DTOs;
using AutoNext.Platform.AccessControl.API.Models.Entities;
using System.Text.Json;

namespace AutoNext.Platform.AccessControl.API.Mappings
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            // Create DTO to Entity
            CreateMap<UserCreateDto, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.LastLoginAt, opt => opt.Ignore())
                .ForMember(dest => dest.EmailVerified, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.PhoneVerified, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.UserRoles, opt => opt.Ignore())
                .ForMember(dest => dest.UserOrganizations, opt => opt.Ignore())
                .ForMember(dest => dest.RefreshTokens, opt => opt.Ignore())
                .ForMember(dest => dest.UserSessions, opt => opt.Ignore())
                .ForMember(dest => dest.Metadata,
                    opt => opt.MapFrom(src => src.Metadata != null
                        ? JsonSerializer.Serialize(src.Metadata, new JsonSerializerOptions { PropertyNamingPolicy = null })
                        : null));

            // Update DTO to Entity
            CreateMap<UserUpdateDto, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Email, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.EmailVerified, opt => opt.Ignore())
                .ForMember(dest => dest.PhoneVerified, opt => opt.Ignore())
                .ForMember(dest => dest.LastLoginAt, opt => opt.Ignore())
                .ForMember(dest => dest.FirstName, opt => opt.Condition(src => !string.IsNullOrEmpty(src.FirstName)))
                .ForMember(dest => dest.LastName, opt => opt.Condition(src => !string.IsNullOrEmpty(src.LastName)))
                .ForMember(dest => dest.PhoneNumber, opt => opt.Condition(src => !string.IsNullOrEmpty(src.PhoneNumber)))
                .ForMember(dest => dest.UserType, opt => opt.Condition(src => src.UserType != null))
                .ForMember(dest => dest.IsActive, opt => opt.Condition(src => src.IsActive.HasValue))
                .ForMember(dest => dest.Metadata,
                    opt => opt.MapFrom(src => src.Metadata != null
                        ? JsonSerializer.Serialize(src.Metadata, new JsonSerializerOptions { PropertyNamingPolicy = null })
                        : null));

            // Entity to Response DTO
            CreateMap<User, UserResponseDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}".Trim()))
                .ForMember(dest => dest.Metadata,
                    opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.Metadata)
                        ? JsonSerializer.Deserialize<Dictionary<string, object>>(src.Metadata, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                        : null))
                .ForMember(dest => dest.Roles, opt => opt.Ignore())
                .ForMember(dest => dest.Permissions, opt => opt.Ignore())
                .ForMember(dest => dest.Organizations, opt => opt.Ignore());

            // Entity to UserInfo DTO
            CreateMap<User, UserInfoDto>()
                .ForMember(dest => dest.Roles, opt => opt.Ignore())
                .ForMember(dest => dest.Permissions, opt => opt.Ignore());
        }
    }
}