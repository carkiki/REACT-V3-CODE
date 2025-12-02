using System;
using System.Security.Cryptography;
using System.Text;

namespace ReactCRM.Utils
{
    public static class HashUtils
    {
        public static string HashPassword(string password, string salt)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password + salt);
                byte[] hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        public static bool VerifyPassword(string password, string hashedPasswordWithSalt)
        {
            string[] parts = hashedPasswordWithSalt.Split(':');
            if (parts.Length != 2) return false;

            string storedHash = parts[0];
            string salt = parts[1];

            string computedHash = HashPassword(password, salt);
            return storedHash == computedHash;
        }

        public static string HashSSN(string ssn)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(ssn);
                byte[] hash = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        public static string GenerateSalt()
        {
            return Guid.NewGuid().ToString();
        }
    }
}