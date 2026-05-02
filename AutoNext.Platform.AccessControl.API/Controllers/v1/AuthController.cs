
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
        [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Error("Invalid request", 400,
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));

            try
            {
                var result = await _authService.RegisterAsync(request);
                return Ok(ApiResponse<AuthResponse>.Ok(result, "Registration successful"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message, 400));
            }
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);
            if (result == null)
                return Unauthorized(ApiResponse<object>.Unauthorized("Invalid email or password"));

            var userId = result.User.Id;
            var is2FAEnabled = await _twoFactorService.IsTwoFactorEnabledAsync(userId);

            if (is2FAEnabled)
            {
                return Ok(ApiResponse<object>.Ok(new
                {
                    requiresTwoFactor = true,
                    userId = userId,
                    message = "Two-factor authentication required"
                }));
            }

            return Ok(ApiResponse<AuthResponse>.Ok(result, "Login successful"));
        }

        [HttpPost("refresh")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            var result = await _authService.RefreshTokenAsync(request);
            if (result == null)
                return Unauthorized(ApiResponse<object>.Unauthorized("Invalid or expired refresh token"));

            return Ok(ApiResponse<AuthResponse>.Ok(result, "Token refreshed successfully"));
        }

        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest? request = null)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
            {
                await _authService.LogoutAsync(Guid.Parse(userId), request?.RefreshToken);
            }

            return Ok(ApiResponse<object>.Ok(null, "Logout successful"));
        }

        [Authorize]
        [HttpPost("change-password")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized(ApiResponse<object>.Unauthorized());

            try
            {
                var result = await _authService.ChangePasswordAsync(Guid.Parse(userId), request);
                if (!result)
                    return BadRequest(ApiResponse<object>.Error("Failed to change password", 400));

                return Ok(ApiResponse<object>.Ok(null, "Password changed successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message, 400));
            }
        }

        [HttpPost("forgot-password")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var response = await _authService.ForgotPasswordAsync(request);
            if(response.IsValid == false)
                return Ok(ApiResponse<object>.Ok(null, "unable to send link"));

            return Ok(ApiResponse<ForgotPasswordResponse>.Ok(response, "link genarated and click on it for redirection to reset password"));
        }

        [HttpPost("reset-password")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                var result = await _authService.ResetPasswordAsync(request);
                if (!result)
                    return BadRequest(ApiResponse<object>.Error("Failed to reset password", 400));

                return Ok(ApiResponse<object>.Ok(true, "Password reset successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message, 400));
            }
        }

        [HttpPost("google")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            try
            {
                var googleUser = await _googleAuthService.ValidateGoogleTokenAsync(request.IdToken);
                if (googleUser == null)
                    return Unauthorized(ApiResponse<object>.Unauthorized("Invalid Google token"));

                var result = await _authService.GoogleLoginAsync(request);

                if (result == null)
                    return Unauthorized(ApiResponse<object>.Unauthorized("Google authentication failed"));

                return Ok(ApiResponse<AuthResponse>.Ok(result, "Google login successful"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Google login");
                return StatusCode(500, ApiResponse<object>.Error("An error occurred during Google login", 500));
            }
        }

        [HttpGet("google/url")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        public IActionResult GetGoogleLoginUrl([FromQuery] string redirectUri)
        {
            var url = _googleAuthService.GetGoogleLoginUrl(redirectUri);
            return Ok(ApiResponse<string>.Ok(url, "Google login URL generated"));
        }

        [HttpPost("send-verification-otp")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SendVerificationOtp([FromBody] SendVerificationOtpRequest request)
        {
            await _authService.SendVerificationOtpAsync(request.Email, request.Purpose);
            return Ok(ApiResponse<object>.Ok(null, "OTP sent successfully"));
        }

        [HttpPost("verify-otp")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            try
            {
                var result = await _authService.VerifyOtpAsync(request);
                if (!result)
                    return BadRequest(ApiResponse<object>.Error("OTP verification failed", 400));

                return Ok(ApiResponse<object>.Ok(null, "OTP verified successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message, 400));
            }
        }

        [Authorize]
        [HttpPost("2fa/disable")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> DisableTwoFactor()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized(ApiResponse<object>.Unauthorized());

            await _twoFactorService.DisableTwoFactorAsync(Guid.Parse(userId));
            return Ok(ApiResponse<object>.Ok(null, "2FA disabled successfully"));
        }

        [Authorize]
        [HttpGet("2fa/status")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTwoFactorStatus()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized(ApiResponse<object>.Unauthorized());

            var isEnabled = await _twoFactorService.IsTwoFactorEnabledAsync(Guid.Parse(userId));
            return Ok(ApiResponse<bool>.Ok(isEnabled, "2FA status retrieved"));
        }
    }
}