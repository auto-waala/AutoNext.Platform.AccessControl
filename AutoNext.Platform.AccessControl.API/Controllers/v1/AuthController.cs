using Asp.Versioning;
using AutoNext.Platform.AccessControl.API.Managers.Interfaces;
using AutoNext.Platform.AccessControl.API.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AutoNext.Platform.AccessControl.API.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly IGoogleAuthService _googleAuthService;
        private readonly ITwoFactorService _twoFactorService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthService authService,
            IUserService userService,
            IGoogleAuthService googleAuthService,
            ITwoFactorService twoFactorService,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _userService = userService;
            _googleAuthService = googleAuthService;
            _twoFactorService = twoFactorService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            _logger.LogInformation("Register attempt for {Email}", request.Email);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid register request for {Email}", request.Email);
                return BadRequest(ApiResponse<object>.Error("Invalid request", 400,
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            try
            {
                var result = await _authService.RegisterAsync(request);
                _logger.LogInformation("User registered successfully: {Email}", request.Email);
                return Ok(ApiResponse<AuthResponse>.Ok(result, "Registration successful"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Registration failed for {Email}", request.Email);
                return BadRequest(ApiResponse<object>.Error(ex.Message, 400));
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            _logger.LogInformation("Login attempt for {Email}", request.Email);

            var result = await _authService.LoginAsync(request);
            if (result == null)
            {
                _logger.LogWarning("Login failed for {Email}", request.Email);
                return Unauthorized(ApiResponse<object>.Unauthorized("Invalid email or password"));
            }

            var userId = result.User.Id;
            var is2FAEnabled = await _twoFactorService.IsTwoFactorEnabledAsync(userId);

            if (is2FAEnabled)
            {
                _logger.LogInformation("2FA required for user {UserId}", userId);
                return Ok(ApiResponse<object>.Ok(new
                {
                    requiresTwoFactor = true,
                    userId = userId
                }));
            }

            _logger.LogInformation("Login successful for {UserId}", userId);
            return Ok(ApiResponse<AuthResponse>.Ok(result, "Login successful"));
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            _logger.LogInformation("Token refresh attempt");

            var result = await _authService.RefreshTokenAsync(request);
            if (result == null)
            {
                _logger.LogWarning("Invalid refresh token");
                return Unauthorized(ApiResponse<object>.Unauthorized("Invalid or expired refresh token"));
            }

            _logger.LogInformation("Token refreshed successfully");
            return Ok(ApiResponse<AuthResponse>.Ok(result, "Token refreshed successfully"));
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest? request = null)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            _logger.LogInformation("Logout attempt for {UserId}", userId);

            if (userId != null)
                await _authService.LogoutAsync(Guid.Parse(userId), request?.RefreshToken);

            return Ok(ApiResponse<object>.Ok(null, "Logout successful"));
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                _logger.LogWarning("Unauthorized password change attempt");
                return Unauthorized(ApiResponse<object>.Unauthorized());
            }

            try
            {
                _logger.LogInformation("Password change attempt for {UserId}", userId);

                var result = await _authService.ChangePasswordAsync(Guid.Parse(userId), request);

                if (!result)
                {
                    _logger.LogWarning("Password change failed for {UserId}", userId);
                    return BadRequest(ApiResponse<object>.Error("Failed to change password", 400));
                }

                _logger.LogInformation("Password changed successfully for {UserId}", userId);
                return Ok(ApiResponse<object>.Ok(null, "Password changed successfully"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Password change error for {UserId}", userId);
                return BadRequest(ApiResponse<object>.Error(ex.Message, 400));
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            _logger.LogInformation("Forgot password request for {Email}", request.Email);

            var response = await _authService.ForgotPasswordAsync(request);

            if (response == null)
            {
                _logger.LogWarning("Failed to generate reset link for {Email}", request.Email);
                return Ok(ApiResponse<object>.Ok(null, "Unable to send link"));
            }

            _logger.LogInformation("Reset link generated for {Email}", request.Email);
            return Ok(ApiResponse<ForgotPasswordResponse>.Ok(response, "Reset link sent"));
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            _logger.LogInformation("Reset password attempt");

            try
            {
                var result = await _authService.ResetPasswordAsync(request);

                if (!result)
                {
                    _logger.LogWarning("Password reset failed");
                    return BadRequest(ApiResponse<object>.Error("Failed to reset password", 400));
                }

                _logger.LogInformation("Password reset successful");
                return Ok(ApiResponse<object>.Ok(true, "Password reset successfully"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Reset password error");
                return BadRequest(ApiResponse<object>.Error(ex.Message, 400));
            }
        }

        [HttpPost("google")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            _logger.LogInformation("Google login attempt");

            try
            {
                var googleUser = await _googleAuthService.ValidateGoogleTokenAsync(request.IdToken);

                if (googleUser == null)
                {
                    _logger.LogWarning("Invalid Google token");
                    return Unauthorized(ApiResponse<object>.Unauthorized("Invalid Google token"));
                }

                var result = await _authService.GoogleLoginAsync(request);

                if (result == null)
                {
                    _logger.LogWarning("Google login failed");
                    return Unauthorized(ApiResponse<object>.Unauthorized("Google authentication failed"));
                }

                _logger.LogInformation("Google login successful for {Email}", googleUser.Email);
                return Ok(ApiResponse<AuthResponse>.Ok(result, "Google login successful"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Google login");
                return StatusCode(500, ApiResponse<object>.Error("Internal server error", 500));
            }
        }

        [HttpPost("send-verification-otp")]
        public async Task<IActionResult> SendVerificationOtp([FromBody] SendVerificationOtpRequest request)
        {
            _logger.LogInformation("Sending OTP to {Email} for {Purpose}", request.Email, request.Purpose);

            await _authService.SendVerificationOtpAsync(request.Email, request.Purpose);

            return Ok(ApiResponse<object>.Ok(null, "OTP sent successfully"));
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            _logger.LogInformation("OTP verification attempt for {Email}", request.Email);

            try
            {
                var result = await _authService.VerifyOtpAsync(request);

                if (!result)
                {
                    _logger.LogWarning("OTP verification failed for {Email}", request.Email);
                    return BadRequest(ApiResponse<object>.Error("OTP verification failed", 400));
                }

                _logger.LogInformation("OTP verified successfully for {Email}", request.Email);
                return Ok(ApiResponse<object>.Ok(null, "OTP verified successfully"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "OTP verification error for {Email}", request.Email);
                return BadRequest(ApiResponse<object>.Error(ex.Message, 400));
            }
        }
    }
}