using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Security.Cryptography;

namespace DocumentManagement.Data.Security
{
    public class PasswordHasher : IPasswordHasher
    {
        public string HashPassword(string plainTextPassword)
        {
            byte[] salt = GenerateSalt();
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: plainTextPassword,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));

            return $"{Convert.ToBase64String(salt)}:{hashed}";
        }

        public bool VerifyPassword(string storedHashedData, string providedPassword)
        {
            var parts = storedHashedData.Split(':', 2);
            if (parts.Length != 2)
            {
                throw new FormatException("The stored password hash is not in the expected format.");
            }

            var salt = Convert.FromBase64String(parts[0]);
            var storedHash = parts[1];

            var hashOfProvided = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: providedPassword,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));

            return storedHash == hashOfProvided;
        }

        private byte[] GenerateSalt()
        {
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }
    }

}
