using AutoNext.Platform.AccessControl.API.Models.Entities;

namespace AutoNext.Platform.AccessControl.API.Managers.Interfaces
{
    public interface IJwtTokenService
    {
        string GenerateAccessToken(User user, IEnumerable<string> roles, IEnumerable<string> permissions);
        string GenerateRefreshToken();
        bool ValidateToken(string token);
        Guid? GetUserIdFromToken(string token);
    }
}
