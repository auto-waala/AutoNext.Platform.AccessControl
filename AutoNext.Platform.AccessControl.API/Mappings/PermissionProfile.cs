using AutoMapper;
using AutoNext.Platform.AccessControl.API.Models.DTOs;
using AutoNext.Platform.AccessControl.API.Models.Entities;

namespace AutoNext.Platform.AccessControl.API.Mappings
{
    public class PermissionProfile : Profile
    {
        public PermissionProfile()
        {
            // Create DTO to Entity
            CreateMap<PermissionCreateDto, Permission>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.RolePermissions, opt => opt.Ignore());

            // Entity to DTO
            CreateMap<Permission, PermissionDto>();

            // Entity to Response DTO
            CreateMap<Permission, PermissionResponseDto>();
        }
    }
}
