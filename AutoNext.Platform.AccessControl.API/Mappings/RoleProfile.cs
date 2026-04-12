using AutoMapper;
using AutoNext.Platform.AccessControl.API.Models.DTOs;
using AutoNext.Platform.AccessControl.API.Models.Entities;

namespace AutoNext.Platform.AccessControl.API.Mappings
{
    public class RoleProfile : Profile
    {
        public RoleProfile()
        {
            // Create DTO to Entity
            CreateMap<RoleCreateDto, Role>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsSystemRole, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.UserRoles, opt => opt.Ignore())
                .ForMember(dest => dest.RolePermissions, opt => opt.Ignore());

            // Update DTO to Entity
            CreateMap<RoleUpdateDto, Role>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Code, opt => opt.Condition(src => src.Code != null))
                .ForMember(dest => dest.Name, opt => opt.Condition(src => src.Name != null))
                .ForMember(dest => dest.Description, opt => opt.Condition(src => src.Description != null))
                .ForMember(dest => dest.DisplayOrder, opt => opt.Condition(src => src.DisplayOrder.HasValue))
                .ForMember(dest => dest.IsActive, opt => opt.Condition(src => src.IsActive.HasValue));

            // Entity to Response DTO
            CreateMap<Role, RoleResponseDto>()
                .ForMember(dest => dest.Permissions, opt => opt.Ignore())
                .ForMember(dest => dest.UserCount, opt => opt.Ignore());
        }
    }
}
