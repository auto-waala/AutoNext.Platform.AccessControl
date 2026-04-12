using AutoNext.Platform.AccessControl.API.Data.UnitOfWork;
using AutoNext.Platform.AccessControl.API.Managers.Interfaces;
using AutoNext.Platform.AccessControl.API.Models.Entities;
using System.Security.Cryptography;
using System.Text;

namespace AutoNext.Platform.AccessControl.API.Managers.Services
{
    public class TwoFactorService : ITwoFactorService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TwoFactorService> _logger;
        private readonly IConfiguration _configuration;

        public TwoFactorService(
            IUnitOfWork unitOfWork,
            ILogger<TwoFactorService> logger,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<bool> EnableTwoFactorAsync(Guid userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                return false;

            // Store 2FA secret in metadata
            var metadata = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(user.Metadata))
            {
                metadata = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(user.Metadata) ?? new();
            }

            var secret = GenerateSecretKey();
            metadata["two_factor_enabled"] = true;
            metadata["two_factor_secret"] = secret;
            metadata["two_factor_enabled_at"] = DateTime.UtcNow;

            user.Metadata = System.Text.Json.JsonSerializer.Serialize(metadata);
            user.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Two-factor authentication enabled for user: {UserId}", userId);
            return true;
        }

        public async Task<bool> DisableTwoFactorAsync(Guid userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                return false;

            var metadata = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(user.Metadata))
            {
                metadata = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(user.Metadata) ?? new();
            }

            metadata["two_factor_enabled"] = false;
            metadata.Remove("two_factor_secret");
            metadata.Remove("two_factor_enabled_at");

            user.Metadata = System.Text.Json.JsonSerializer.Serialize(metadata);
            user.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Two-factor authentication disabled for user: {UserId}", userId);
            return true;
        }

        public async Task<bool> IsTwoFactorEnabledAsync(Guid userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                return false;

            if (string.IsNullOrEmpty(user.Metadata))
                return false;

            var metadata = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(user.Metadata);
            return metadata != null && metadata.ContainsKey("two_factor_enabled") && (bool)metadata["two_factor_enabled"];
        }

        public async Task<string> GenerateTwoFactorCodeAsync(User user)
        {
            if (!await IsTwoFactorEnabledAsync(user.Id))
                throw new InvalidOperationException("Two-factor authentication is not enabled");

            var secret = GetSecretFromUser(user);
            var code = GenerateTotpCode(secret);

            // Store code temporarily for validation
            var metadata = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(user.Metadata))
            {
                metadata = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(user.Metadata) ?? new();
            }

            metadata["two_factor_current_code"] = code;
            metadata["two_factor_code_generated_at"] = DateTime.UtcNow;

            user.Metadata = System.Text.Json.JsonSerializer.Serialize(metadata);
            await _unitOfWork.SaveChangesAsync();

            return code;
        }

        public async Task<bool> ValidateTwoFactorCodeAsync(User user, string code)
        {
            if (string.IsNullOrEmpty(user.Metadata))
                return false;

            var metadata = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(user.Metadata);
            if (metadata == null)
                return false;

            var generatedAt = metadata.ContainsKey("two_factor_code_generated_at")
                ? DateTime.Parse(metadata["two_factor_code_generated_at"].ToString()!)
                : DateTime.MinValue;

            var storedCode = metadata.ContainsKey("two_factor_current_code")
                ? metadata["two_factor_current_code"].ToString()
                : null;

            // Code expires after 5 minutes
            if ((DateTime.UtcNow - generatedAt).TotalMinutes > 5)
                return false;

            return storedCode == code;
        }

        public async Task<string> GenerateRecoveryCodesAsync(Guid userId, int count = 10)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found");

            var recoveryCodes = new List<string>();
            for (int i = 0; i < count; i++)
            {
                recoveryCodes.Add(GenerateRecoveryCode());
            }

            var metadata = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(user.Metadata))
            {
                metadata = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(user.Metadata) ?? new();
            }

            metadata["two_factor_recovery_codes"] = recoveryCodes.Select(c => HashCode(c)).ToList();
            metadata["two_factor_recovery_codes_generated_at"] = DateTime.UtcNow;

            user.Metadata = System.Text.Json.JsonSerializer.Serialize(metadata);
            await _unitOfWork.SaveChangesAsync();

            return string.Join("\n", recoveryCodes);
        }

        public async Task<bool> ValidateRecoveryCodeAsync(Guid userId, string recoveryCode)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.Metadata))
                return false;

            var metadata = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(user.Metadata);
            if (metadata == null || !metadata.ContainsKey("two_factor_recovery_codes"))
                return false;

            var recoveryCodes = System.Text.Json.JsonSerializer.Deserialize<List<string>>(metadata["two_factor_recovery_codes"].ToString()!);
            var hashedCode = HashCode(recoveryCode);

            if (recoveryCodes != null && recoveryCodes.Contains(hashedCode))
            {
                recoveryCodes.Remove(hashedCode);
                metadata["two_factor_recovery_codes"] = recoveryCodes;
                user.Metadata = System.Text.Json.JsonSerializer.Serialize(metadata);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }

            return false;
        }

        private string GenerateSecretKey()
        {
            var key = new byte[20];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(key);
            return Convert.ToBase64String(key);
        }

        private string GenerateTotpCode(string secret)
        {
            // Simple TOTP implementation
            var epoch = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var counter = (long)epoch.TotalSeconds / 30;

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var counterBytes = BitConverter.GetBytes(counter);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(counterBytes);

            var hash = hmac.ComputeHash(counterBytes);
            var offset = hash[^1] & 0x0F;
            var code = (hash[offset] & 0x7F) << 24 |
                       (hash[offset + 1] & 0xFF) << 16 |
                       (hash[offset + 2] & 0xFF) << 8 |
                       (hash[offset + 3] & 0xFF);

            return (code % 1000000).ToString("D6");
        }

        private string GenerateRecoveryCode()
        {
            var bytes = new byte[10];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Substring(0, 10);
        }

        private string HashCode(string code)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(code));
            return Convert.ToBase64String(hash);
        }

        private string GetSecretFromUser(User user)
        {
            if (string.IsNullOrEmpty(user.Metadata))
                return string.Empty;

            var metadata = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(user.Metadata);
            return metadata?.ContainsKey("two_factor_secret") == true ? metadata["two_factor_secret"].ToString()! : string.Empty;
        }
    }
}
