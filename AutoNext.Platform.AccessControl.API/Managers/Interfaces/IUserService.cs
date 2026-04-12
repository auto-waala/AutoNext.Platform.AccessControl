using AutoNext.Platform.AccessControl.API.Models.DTOs;

namespace AutoNext.Platform.AccessControl.API.Managers.Interfaces
{
    public interface IUserService
    {
        Task<UserResponseDto?> GetUserByIdAsync(Guid userId);
        Task<UserResponseDto?> GetUserByEmailAsync(string email);
        Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();
        Task<IEnumerable<UserResponseDto>> GetUsersByTypeAsync(string userType);
        Task<IEnumerable<UserResponseDto>> GetUsersByOrganizationAsync(Guid organizationId);
        Task<UserResponseDto> CreateUserAsync(UserCreateDto createDto);
        Task<UserResponseDto?> UpdateUserAsync(Guid userId, UserUpdateDto updateDto);
        Task<bool> DeleteUserAsync(Guid userId);
        Task<bool> ToggleUserStatusAsync(Guid userId, bool isActive);
        Task<bool> AssignRolesAsync(Guid userId, List<Guid> roleIds);
        Task<bool> AssignOrganizationAsync(Guid userId, Guid organizationId, bool isPrimary);
    }
}
