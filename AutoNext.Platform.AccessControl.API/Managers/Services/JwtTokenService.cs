using AutoNext.Platform.AccessControl.API.Managers.Interfaces;
using AutoNext.Platform.AccessControl.API.Models.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AutoNext.Platform.AccessControl.API.Managers.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _configuration;

        public JwtTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateAccessToken(User user, IEnumerable<string> roles, IEnumerable<string> permissions)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new Claim("user_type", user.UserType),
                new Claim("email_verified", user.EmailVerified.ToString()),
                new Claim("user_id", user.Id.ToString())
            };

            if (!string.IsNullOrEmpty(user.FirstName))
                claims.Add(new Claim("first_name", user.FirstName));

            if (!string.IsNullOrEmpty(user.LastName))
                claims.Add(new Claim("last_name", user.LastName));

            if (!string.IsNullOrEmpty(user.PhoneNumber))
                claims.Add(new Claim("phone_number", user.PhoneNumber));

            // Add roles
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
                claims.Add(new Claim("role", role));
            }

            // Add permissions
            foreach (var permission in permissions)
            {
                claims.Add(new Claim("permission", permission));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiresIn = Convert.ToDouble(_configuration["Jwt:ExpiryMinutes"] ?? "15");

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiresIn),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public bool ValidateToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                return false;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out _);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public Guid? GetUserIdFromToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var jwtToken = tokenHandler.ReadJwtToken(token);
                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);

                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                    return userId;

                // Try alternative claim
                var altUserIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "user_id");
                if (altUserIdClaim != null && Guid.TryParse(altUserIdClaim.Value, out var altUserId))
                    return altUserId;

                return null;
            }
            catch
            {
                return null;
            }
        }

        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);

            try
            {
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = false, // Don't validate lifetime for expired token
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out _);
                return principal;
            }
            catch
            {
                return null;
            }
        }

        public DateTime GetTokenExpiry(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            var expiryClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp);

            if (expiryClaim != null && long.TryParse(expiryClaim.Value, out var expiryUnix))
            {
                return DateTimeOffset.FromUnixTimeSeconds(expiryUnix).UtcDateTime;
            }

            return DateTime.UtcNow;
        }
    }
}
