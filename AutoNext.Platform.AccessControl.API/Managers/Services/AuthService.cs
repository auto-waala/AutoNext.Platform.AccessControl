using AutoMapper;
using AutoNext.Platform.AccessControl.API.Data.UnitOfWork;
using AutoNext.Platform.AccessControl.API.Managers.Interfaces;
using AutoNext.Platform.AccessControl.API.Models.DTOs;
using AutoNext.Platform.AccessControl.API.Models.Entities;
using System.Security.Cryptography;
using System.Text;

namespace AutoNext.Platform.AccessControl.API.Managers.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtTokenService _jwtService;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUnitOfWork unitOfWork,
            IJwtTokenService jwtService,
            IEmailService emailService,
            IMapper mapper,
            ILogger<AuthService> logger)
        {
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
            _emailService = emailService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Check Existing User
                var existingUser = await _unitOfWork.Users.GetByEmailAsync(request.Email);
                if (existingUser != null)
                    throw new InvalidOperationException("User with this email already exists");

                // Create User
                var user = new User
                {
                    Email = request.Email.Trim().ToLower(),
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    PhoneNumber = request.PhoneNumber,
                    UserType = request.UserType ?? "Customer",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    EmailVerified = false,
                    PhoneVerified = false
                };

                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();

                // Assign Default Role
                var roleCode = user.UserType.ToLower();

                var defaultRole = await _unitOfWork.Roles.GetByCodeAsync(roleCode);

                if (defaultRole != null)
                {
                    var userRole = new UserRole
                    {
                        UserId = user.Id,
                        RoleId = defaultRole.Id,
                        AssignedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.UserRoles.AddUserRoleAsync(userRole);
                    await _unitOfWork.SaveChangesAsync();
                }

                // Create Personal Organization For Customer
                if (user.UserType.Equals("Customer", StringComparison.OrdinalIgnoreCase))
                {
                    var organization = new Organization
                    {
                        Name = $"{user.FirstName} {user.LastName}".Trim(),
                        Code = $"user_{user.Id:N}",
                        OrganizationType = "Individual",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.Organizations.AddAsync(organization);
                    await _unitOfWork.SaveChangesAsync();

                    await _unitOfWork.UserOrganizations.SetPrimaryOrganizationAsync(
                        user.Id,
                        organization.Id
                    );

                    await _unitOfWork.SaveChangesAsync();
                }

                // Generate Tokens
                var roles = await GetUserRolesAsync(user.Id);
                var permissions = await GetUserPermissionsAsync(user.Id);

                var accessToken = _jwtService.GenerateAccessToken(user, roles, permissions);
                var refreshToken = await GenerateAndStoreRefreshTokenAsync(user.Id);

                await _unitOfWork.CommitTransactionAsync();

                //await _emailService.SendWelcomeEmailAsync(
                //    user.Email,
                //    user.FirstName ?? user.Email
                //);

                _logger.LogInformation("User registered successfully: {Email}", user.Email);

                return new AuthResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                    User = new UserInfoDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FirstName = user.FirstName ?? string.Empty,
                        LastName = user.LastName ?? string.Empty,
                        UserType = user.UserType,
                        Roles = roles.ToList(),
                        Permissions = permissions.ToList()
                    }
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();

                _logger.LogError(ex, "Registration failed");

                throw new Exception(
                    $"Registration failed: {ex.InnerException?.Message ?? ex.Message}",
                    ex
                );
            }
        }

        public async Task<AuthResponse?> LoginAsync(LoginRequest request)
        {

            var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return null;

            if (!user.IsActive)
                throw new InvalidOperationException("Account is deactivated");

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Update last login
                user.LastLoginAt = DateTime.UtcNow;
                _unitOfWork.Users.Update(user);
                await _unitOfWork.SaveChangesAsync();

                // Generate tokens
                var roles = await GetUserRolesAsync(user.Id);
                var permissions = await GetUserPermissionsAsync(user.Id);
                var accessToken = _jwtService.GenerateAccessToken(user, roles, permissions);
                var refreshToken = await GenerateAndStoreRefreshTokenAsync(user.Id);

                // Track session
                await TrackUserSessionAsync(user.Id, accessToken);

                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("User logged in successfully: {Email}", user.Email);

                return new AuthResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                    User = new UserInfoDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FirstName = user.FirstName ?? string.Empty,
                        LastName = user.LastName ?? string.Empty,
                        UserType = user.UserType,
                        Roles = roles.ToList(),
                        Permissions = permissions.ToList()
                    }
                };
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<AuthResponse?> RefreshTokenAsync(RefreshTokenRequest request)
        {
            var tokenHash = HashToken(request.RefreshToken);
            var refreshToken = await _unitOfWork.RefreshTokens.GetByTokenHashAsync(tokenHash);

            if (refreshToken == null || refreshToken.IsRevoked || refreshToken.ExpiresAt < DateTime.UtcNow)
                return null;

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Revoke old token
                refreshToken.IsRevoked = true;
                refreshToken.RevokedAt = DateTime.UtcNow;
                _unitOfWork.RefreshTokens.Update(refreshToken);

                // Generate new tokens
                var user = refreshToken.User!;
                var roles = await GetUserRolesAsync(user.Id);
                var permissions = await GetUserPermissionsAsync(user.Id);
                var accessToken = _jwtService.GenerateAccessToken(user, roles, permissions);
                var newRefreshToken = await GenerateAndStoreRefreshTokenAsync(user.Id);

                await _unitOfWork.CommitTransactionAsync();

                return new AuthResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = newRefreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                    User = new UserInfoDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FirstName = user.FirstName ?? string.Empty,
                        LastName = user.LastName ?? string.Empty,
                        UserType = user.UserType,
                        Roles = roles.ToList(),
                        Permissions = permissions.ToList()
                    }
                };
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        // Private helper methods
        private async Task<IEnumerable<string>> GetUserRolesAsync(Guid userId)
        {
            var roles = await _unitOfWork.Roles.GetRolesByUserAsync(userId);
            return roles.Select(r => r.Code);
        }

        private async Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId)
        {
            return await _unitOfWork.Permissions.GetPermissionCodesByUserAsync(userId);
        }

        private async Task<string> GenerateAndStoreRefreshTokenAsync(Guid userId)
        {
            var rawToken = _jwtService.GenerateRefreshToken();
            var tokenHash = HashToken(rawToken);

            var refreshToken = new RefreshToken
            {
                UserId = userId,
                TokenHash = tokenHash,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.RefreshTokens.AddAsync(refreshToken);
            await _unitOfWork.SaveChangesAsync();

            return rawToken;
        }

        private async Task TrackUserSessionAsync(Guid userId, string accessToken)
        {
            var session = new UserSession
            {
                UserId = userId,
                AccessTokenHash = HashToken(accessToken),
                ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.UserSessions.AddAsync(session);
            await _unitOfWork.SaveChangesAsync();
        }

        private string HashToken(string token)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(hash);
        }

        public async Task<bool> LogoutAsync(Guid userId, string refreshToken)
        {
            var tokenHash = HashToken(refreshToken);

            var existingToken = await _unitOfWork.RefreshTokens.GetByTokenHashAsync(tokenHash);

            if (existingToken == null || existingToken.UserId != userId)
                return false;

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                existingToken.IsRevoked = true;
                existingToken.RevokedAt = DateTime.UtcNow;

                _unitOfWork.RefreshTokens.Update(existingToken);

                // Optional: Remove all active sessions
                await _unitOfWork.UserSessions.InvalidateUserSessionsAsync(userId);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return true;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);

            if (user == null)
                return false;

            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                throw new InvalidOperationException("Invalid current password");

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Users.Update(user);

                // Revoke all sessions (force re-login)
                await _unitOfWork.UserSessions.InvalidateUserSessionsAsync(userId);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return true;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<ForgotPasswordResponse> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);

            // Always same response (security)
            if (user == null)
            {
                return new ForgotPasswordResponse
                {
                    IsValid = false,
                    ResetPasswordToken = string.Empty
                };
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // 🔥 Invalidate old tokens (important)
                await _unitOfWork.PasswordResetTokens.InvalidateUserTokensAsync(user.Id);

                var resetToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
                var tokenHash = HashToken(resetToken);

                var resetEntity = new PasswordResetToken
                {
                    UserId = user.Id,
                    TokenHash = tokenHash,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                    CreatedAt = DateTime.UtcNow,
                    IsUsed = false
                };

                await _unitOfWork.PasswordResetTokens.AddAsync(resetEntity);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                return new ForgotPasswordResponse
                {
                    IsValid = true,
                    ResetPasswordToken = resetToken
                };
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var tokenHash = HashToken(request.Token);

            // 🔥 Use repository validation method
            var token = await _unitOfWork.PasswordResetTokens
                .GetValidTokenAsync(tokenHash);

            if (token == null)
                return false;

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(token.UserId);

                if (user == null)
                    return false;

                // Update password
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;

                // 🔥 Mark token as used via repository
                await _unitOfWork.PasswordResetTokens.MarkAsUsedAsync(token.Id);

                // 🔥 Invalidate all sessions
                await _unitOfWork.UserSessions.InvalidateUserSessionsAsync(user.Id);

                _unitOfWork.Users.Update(user);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return true;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public Task<AuthResponse?> GoogleLoginAsync(GoogleLoginRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SendVerificationOtpAsync(string email, string purpose)
        {
            throw new NotImplementedException();
        }

        public Task<bool> VerifyOtpAsync(VerifyOtpRequest request)
        {
            throw new NotImplementedException();
        }

        // Additional methods (Logout, ChangePassword, ForgotPassword, etc.) follow same pattern
        // ...
    }
}
