using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace CampusEventHub.Helpers
{
    public static class PasswordHelper
    {
        // Hash password và trả về: {salt}.{hash}
        public static string HashPassword(string password)
        {
            // Tạo salt random 128 bit
            byte[] salt = RandomNumberGenerator.GetBytes(128 / 8);

            // Hash với PBKDF2
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            // Lưu salt + hash chung lại
            return $"{Convert.ToBase64String(salt)}.{hashed}";
        }

        // Kiểm tra password nhập có đúng không
        public static bool VerifyPassword(string password, string storedHash)
        {
            var parts = storedHash.Split('.');
            if (parts.Length != 2)
                return false;

            byte[] salt = Convert.FromBase64String(parts[0]);
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            return hashed == parts[1];
        }
    }
}