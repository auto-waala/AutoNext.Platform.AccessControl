using System.Security.Cryptography;
using System.Text;

namespace AutoNext.Platform.AccessControl.API.Helpers
{
    public static class PasswordHasher
    {
        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
        }

        public static bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

        public static string GenerateRandomPassword(int length = 12)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()";
            var result = new StringBuilder();

            using (var rng = RandomNumberGenerator.Create())
            {
                var buffer = new byte[sizeof(uint)];

                for (int i = 0; i < length; i++)
                {
                    rng.GetBytes(buffer);
                    var num = BitConverter.ToUInt32(buffer, 0);
                    result.Append(validChars[(int)(num % (uint)validChars.Length)]);
                }
            }

            return result.ToString();
        }

        public static bool IsStrongPassword(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 8)
                return false;

            var hasUpper = false;
            var hasLower = false;
            var hasDigit = false;
            var hasSpecial = false;

            foreach (var c in password)
            {
                if (char.IsUpper(c)) hasUpper = true;
                else if (char.IsLower(c)) hasLower = true;
                else if (char.IsDigit(c)) hasDigit = true;
                else if (char.IsPunctuation(c) || char.IsSymbol(c)) hasSpecial = true;
            }

            return hasUpper && hasLower && hasDigit && hasSpecial;
        }
    }
}
