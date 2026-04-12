using AutoMapper;
using AutoNext.Platform.AccessControl.API.Models.DTOs;
using AutoNext.Platform.AccessControl.API.Models.Entities;
using System.Text.Json; 


namespace AutoNext.Platform.AccessControl.API.Mappings
{
    public class OrganizationProfile : Profile
    {
        public OrganizationProfile()
        {
            // Create DTO to Entity
            CreateMap<OrganizationCreateDto, Organization>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.UserOrganizations, opt => opt.Ignore())
                .ForMember(dest => dest.Metadata,
                    opt => opt.MapFrom(src => src.Metadata != null
                        ? JsonSerializer.Serialize(src.Metadata, new JsonSerializerOptions { PropertyNamingPolicy = null })
                        : null));

            // Update DTO to Entity
            CreateMap<OrganizationUpdateDto, Organization>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Name, opt => opt.Condition(src => !string.IsNullOrEmpty(src.Name)))
                .ForMember(dest => dest.Code, opt => opt.Condition(src => !string.IsNullOrEmpty(src.Code)))
                .ForMember(dest => dest.OrganizationType, opt => opt.Condition(src => src.OrganizationType != null))
                .ForMember(dest => dest.Address, opt => opt.Condition(src => src.Address != null))
                .ForMember(dest => dest.ContactEmail, opt => opt.Condition(src => !string.IsNullOrEmpty(src.ContactEmail)))
                .ForMember(dest => dest.ContactPhone, opt => opt.Condition(src => !string.IsNullOrEmpty(src.ContactPhone)))
                .ForMember(dest => dest.IsActive, opt => opt.Condition(src => src.IsActive.HasValue))
                .ForMember(dest => dest.Metadata,
                    opt => opt.MapFrom(src => src.Metadata != null
                        ? JsonSerializer.Serialize(src.Metadata, new JsonSerializerOptions { PropertyNamingPolicy = null })
                        : null));

            // Entity to Response DTO
            CreateMap<Organization, OrganizationResponseDto>()
                .ForMember(dest => dest.Metadata,
                    opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.Metadata)
                        ? JsonSerializer.Deserialize<Dictionary<string, object>>(src.Metadata, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                        : null))
                .ForMember(dest => dest.Users, opt => opt.Ignore())
                .ForMember(dest => dest.UserCount, opt => opt.Ignore());

            // Entity to Organization DTO
            CreateMap<Organization, OrganizationDto>();
        }
    }
}