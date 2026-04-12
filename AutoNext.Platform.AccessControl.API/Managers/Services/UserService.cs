
using AutoMapper;
using AutoNext.Platform.AccessControl.API.Data.UnitOfWork;
using AutoNext.Platform.AccessControl.API.Managers.Interfaces;
using AutoNext.Platform.AccessControl.API.Models.DTOs;
using AutoNext.Platform.AccessControl.API.Models.Entities;
using BCrypt.Net;   


namespace AutoNext.Platform.AccessControl.API.Managers.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<UserService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<UserResponseDto?> GetUserByIdAsync(Guid userId)
        {
            var user = await _unitOfWork.Users.GetUserWithRolesAsync(userId);
            if (user == null)
                return null;

            var dto = _mapper.Map<UserResponseDto>(user);

            // Load roles
            var roles = await _unitOfWork.UserRoles.GetUserRolesByUserAsync(userId);
            dto.Roles = roles.Select(r => r.Role!.Name).ToList();

            // Load permissions
            var permissions = await _unitOfWork.Permissions.GetPermissionCodesByUserAsync(userId);
            dto.Permissions = permissions.ToList();

            // Load organizations
            var organizations = await _unitOfWork.UserOrganizations.GetUserOrganizationsByUserAsync(userId);
            dto.Organizations = _mapper.Map<List<OrganizationDto>>(organizations.Select(o => o.Organization));

            return dto;
        }

        public async Task<UserResponseDto?> GetUserByEmailAsync(string email)
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(email);
            if (user == null)
                return null;

            return await GetUserByIdAsync(user.Id);
        }

        public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
        {
            var users = await _unitOfWork.Users.GetAllAsync();
            var result = new List<UserResponseDto>();

            foreach (var user in users)
            {
                var dto = await GetUserByIdAsync(user.Id);
                if (dto != null)
                    result.Add(dto);
            }

            return result;
        }

        public async Task<IEnumerable<UserResponseDto>> GetUsersByTypeAsync(string userType)
        {
            var users = await _unitOfWork.Users.GetUsersByTypeAsync(userType);
            var result = new List<UserResponseDto>();

            foreach (var user in users)
            {
                var dto = await GetUserByIdAsync(user.Id);
                if (dto != null)
                    result.Add(dto);
            }

            return result;
        }

        public async Task<IEnumerable<UserResponseDto>> GetUsersByOrganizationAsync(Guid organizationId)
        {
            var users = await _unitOfWork.Users.GetUsersByOrganizationAsync(organizationId);
            var result = new List<UserResponseDto>();

            foreach (var user in users)
            {
                var dto = await GetUserByIdAsync(user.Id);
                if (dto != null)
                    result.Add(dto);
            }

            return result;
        }

        public async Task<UserResponseDto> CreateUserAsync(UserCreateDto createDto)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Check if email exists
                var existingUser = await _unitOfWork.Users.GetByEmailAsync(createDto.Email);
                if (existingUser != null)
                    throw new InvalidOperationException($"User with email {createDto.Email} already exists");

                var user = _mapper.Map<User>(createDto);
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(createDto.Password);
                user.CreatedAt = DateTime.UtcNow;

                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();

                // Assign roles
                if (createDto.RoleIds != null && createDto.RoleIds.Any())
                {
                    await AssignRolesAsync(user.Id, createDto.RoleIds);
                }

                // Assign organization
                if (createDto.OrganizationId.HasValue)
                {
                    await AssignOrganizationAsync(user.Id, createDto.OrganizationId.Value, true);
                }

                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("User created successfully: {Email}", user.Email);

                return await GetUserByIdAsync(user.Id) ?? throw new Exception("Failed to retrieve created user");
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<UserResponseDto?> UpdateUserAsync(Guid userId, UserUpdateDto updateDto)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                    return null;

                _mapper.Map(updateDto, user);
                user.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Users.Update(user);
                await _unitOfWork.SaveChangesAsync();

                // Update roles if provided
                if (updateDto.RoleIds != null)
                {
                    await AssignRolesAsync(userId, updateDto.RoleIds);
                }

                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("User updated successfully: {Email}", user.Email);

                return await GetUserByIdAsync(userId);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<bool> DeleteUserAsync(Guid userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                return false;

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Remove all relationships
                await _unitOfWork.UserRoles.RemoveUserRolesAsync(userId);
                await _unitOfWork.UserOrganizations.RemoveUserFromOrganizationAsync(userId, Guid.Empty);
                await _unitOfWork.RefreshTokens.RevokeAllUserTokensAsync(userId);
                await _unitOfWork.UserSessions.InvalidateUserSessionsAsync(userId);

                _unitOfWork.Users.Remove(user);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("User deleted successfully: {UserId}", userId);
                return true;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<bool> ToggleUserStatusAsync(Guid userId, bool isActive)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                return false;

            user.IsActive = isActive;
            user.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("User {UserId} status changed to: {Status}", userId, isActive);
            return true;
        }

        public async Task<bool> AssignRolesAsync(Guid userId, List<Guid> roleIds)
        {
            // Remove existing roles
            await _unitOfWork.UserRoles.RemoveUserRolesAsync(userId);

            // Add new roles
            foreach (var roleId in roleIds)
            {
                var userRole = new UserRole
                {
                    UserId = userId,
                    RoleId = roleId,
                    AssignedAt = DateTime.UtcNow
                };
                await _unitOfWork.UserRoles.AddAsync(userRole);
            }

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AssignOrganizationAsync(Guid userId, Guid organizationId, bool isPrimary)
        {
            var existing = await _unitOfWork.UserOrganizations.GetUserOrganizationAsync(userId, organizationId);

            if (existing != null)
            {
                if (isPrimary)
                {
                    await _unitOfWork.UserOrganizations.SetPrimaryOrganizationAsync(userId, organizationId);
                }
                return true;
            }

            var userOrg = new UserOrganization
            {
                UserId = userId,
                OrganizationId = organizationId,
                IsPrimary = isPrimary,
                Status = "Active",
                JoinedAt = DateTime.UtcNow
            };

            await _unitOfWork.UserOrganizations.AddAsync(userOrg);

            if (isPrimary)
            {
                await _unitOfWork.UserOrganizations.SetPrimaryOrganizationAsync(userId, organizationId);
            }

            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }

}
