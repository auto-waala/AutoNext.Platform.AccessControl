using AutoNext.Platform.AccessControl.API.Managers.Interfaces;
using AutoNext.Platform.AccessControl.API.Models.DTOs;
using Google.Apis.Auth;

namespace AutoNext.Platform.AccessControl.API.Managers.Services
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GoogleAuthService> _logger;

        public GoogleAuthService(IConfiguration configuration, ILogger<GoogleAuthService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<GoogleUserInfo?> ValidateGoogleTokenAsync(string idToken)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _configuration["GoogleAuth:ClientId"] }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

                if (payload == null)
                    return null;

                return new GoogleUserInfo
                {
                    Id = payload.Subject,
                    Email = payload.Email,
                    EmailVerified = payload.EmailVerified,
                    Name = payload.Name,
                    GivenName = payload.GivenName,
                    FamilyName = payload.FamilyName,
                    Picture = payload.Picture,
                    Locale = payload.Locale
                };
            }
            catch (InvalidJwtException ex)
            {
                _logger.LogError(ex, "Invalid Google JWT token");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating Google token");
                return null;
            }
        }

        public string GetGoogleLoginUrl(string redirectUri)
        {
            var clientId = _configuration["GoogleAuth:ClientId"];
            var scope = "email profile";

            return $"https://accounts.google.com/o/oauth2/v2/auth?" +
                   $"client_id={clientId}&" +
                   $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" +
                   $"response_type=code&" +
                   $"scope={Uri.EscapeDataString(scope)}";
        }

        public async Task<GoogleUserInfo?> ExchangeCodeForTokenAsync(string code, string redirectUri)
        {
            // This would require HTTP client to exchange code for token
            // Implementation depends on your needs
            throw new NotImplementedException();
        }
    }
}
