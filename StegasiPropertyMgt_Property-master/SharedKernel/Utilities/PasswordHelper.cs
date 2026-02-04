// SharedKernel/Utilities/PasswordHelper.cs
using System;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

namespace SharedKernel.Utilities
{
    public static class PasswordHelper
    {
        /// <summary>
        /// Hashes a password using SHA256.
        /// </summary>
        public static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);

            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Verifies a password against its hashed value.
        /// </summary>
        public static bool VerifyPasswordHash(string password, string hashedPassword)
        {
            var computedHash = HashPassword(password);
            return computedHash == hashedPassword;
        }
    }
}