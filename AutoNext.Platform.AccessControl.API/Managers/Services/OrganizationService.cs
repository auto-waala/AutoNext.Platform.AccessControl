using AutoMapper;
using AutoNext.Platform.AccessControl.API.Data.UnitOfWork;
using AutoNext.Platform.AccessControl.API.Managers.Interfaces;
using AutoNext.Platform.AccessControl.API.Models.DTOs;
using AutoNext.Platform.AccessControl.API.Models.Entities;

namespace AutoNext.Platform.AccessControl.API.Managers.Services
{
    public class OrganizationService : IOrganizationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<OrganizationService> _logger;

        public OrganizationService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<OrganizationService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<OrganizationResponseDto?> GetOrganizationByIdAsync(Guid organizationId)
        {
            var organization = await _unitOfWork.Organizations.GetOrganizationWithUsersAsync(organizationId);
            if (organization == null)
                return null;

            var dto = _mapper.Map<OrganizationResponseDto>(organization);

            // Load users
            var userOrgs = await _unitOfWork.UserOrganizations.GetUserOrganizationsByOrganizationAsync(organizationId);
            dto.Users = _mapper.Map<List<UserResponseDto>>(userOrgs.Select(uo => uo.User));
            dto.UserCount = dto.Users.Count;

            return dto;
        }

        public async Task<OrganizationResponseDto?> GetOrganizationByCodeAsync(string code)
        {
            var organization = await _unitOfWork.Organizations.GetByCodeAsync(code);
            if (organization == null)
                return null;

            return await GetOrganizationByIdAsync(organization.Id);
        }

        public async Task<IEnumerable<OrganizationResponseDto>> GetAllOrganizationsAsync()
        {
            var organizations = await _unitOfWork.Organizations.GetAllAsync();
            var result = new List<OrganizationResponseDto>();

            foreach (var org in organizations)
            {
                var dto = await GetOrganizationByIdAsync(org.Id);
                if (dto != null)
                    result.Add(dto);
            }

            return result;
        }

        public async Task<IEnumerable<OrganizationResponseDto>> GetOrganizationsByTypeAsync(string organizationType)
        {
            var organizations = await _unitOfWork.Organizations.GetOrganizationsByTypeAsync(organizationType);
            var result = new List<OrganizationResponseDto>();

            foreach (var org in organizations)
            {
                var dto = await GetOrganizationByIdAsync(org.Id);
                if (dto != null)
                    result.Add(dto);
            }

            return result;
        }

        public async Task<OrganizationResponseDto> CreateOrganizationAsync(OrganizationCreateDto createDto)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Check if code exists
                var existingOrg = await _unitOfWork.Organizations.GetByCodeAsync(createDto.Code);
                if (existingOrg != null)
                    throw new InvalidOperationException($"Organization with code {createDto.Code} already exists");

                var organization = _mapper.Map<Organization>(createDto);
                organization.CreatedAt = DateTime.UtcNow;

                await _unitOfWork.Organizations.AddAsync(organization);
                await _unitOfWork.SaveChangesAsync();

                // Add admin users
                if (createDto.AdminUserIds != null && createDto.AdminUserIds.Any())
                {
                    foreach (var adminId in createDto.AdminUserIds)
                    {
                        await AddUserToOrganizationAsync(adminId, organization.Id, "Admin");
                    }
                }

                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Organization created successfully: {Code}", organization.Code);

                return await GetOrganizationByIdAsync(organization.Id) ?? throw new Exception("Failed to retrieve created organization");
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<OrganizationResponseDto?> UpdateOrganizationAsync(Guid organizationId, OrganizationUpdateDto updateDto)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var organization = await _unitOfWork.Organizations.GetByIdAsync(organizationId);
                if (organization == null)
                    return null;

                // Check code uniqueness
                if (!string.IsNullOrEmpty(updateDto.Code) && updateDto.Code != organization.Code)
                {
                    var existingOrg = await _unitOfWork.Organizations.GetByCodeAsync(updateDto.Code);
                    if (existingOrg != null)
                        throw new InvalidOperationException($"Organization with code {updateDto.Code} already exists");
                }

                _mapper.Map(updateDto, organization);
                organization.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Organizations.Update(organization);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Organization updated successfully: {Code}", organization.Code);

                return await GetOrganizationByIdAsync(organizationId);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<bool> DeleteOrganizationAsync(Guid organizationId)
        {
            var organization = await _unitOfWork.Organizations.GetByIdAsync(organizationId);
            if (organization == null)
                return false;

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Remove all user associations
                var userOrgs = await _unitOfWork.UserOrganizations
                    .FindAsync(uo => uo.OrganizationId == organizationId);

                if (userOrgs.Any())
                {
                    _unitOfWork.UserOrganizations.RemoveRange(userOrgs);
                }

                _unitOfWork.Organizations.Remove(organization);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Organization deleted successfully: {Code}", organization.Code);
                return true;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<bool> ToggleOrganizationStatusAsync(Guid organizationId, bool isActive)
        {
            var organization = await _unitOfWork.Organizations.GetByIdAsync(organizationId);
            if (organization == null)
                return false;

            organization.IsActive = isActive;
            organization.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Organizations.Update(organization);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Organization {OrgId} status changed to: {Status}", organizationId, isActive);
            return true;
        }

        public async Task<bool> AddUserToOrganizationAsync(Guid userId, Guid organizationId, string role = "Member")
        {
            var existing = await _unitOfWork.UserOrganizations.GetUserOrganizationAsync(userId, organizationId);

            if (existing != null)
                return true;

            var userOrg = new UserOrganization
            {
                UserId = userId,
                OrganizationId = organizationId,
                IsPrimary = false,
                Status = "Active",
                JoinedAt = DateTime.UtcNow
            };

            await _unitOfWork.UserOrganizations.AddAsync(userOrg);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RemoveUserFromOrganizationAsync(Guid userId, Guid organizationId)
        {
            await _unitOfWork.UserOrganizations.RemoveUserFromOrganizationAsync(userId, organizationId);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
