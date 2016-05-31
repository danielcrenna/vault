using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace cohort.Services
{
    /// <summary>
    /// Provides a reasonable default implementation for security functions.
    /// Salted hashing code is sourced from <see href="http://crackstation.net/hashing-security.htm">here.</see>
    /// </summary>
    public class SecurityService : ISecurityService
    {
        public const int SaltLength = 24;
        public const int HashLength = 24;
        public const int HashIterations = 1000;

        public const int IterationIndex = 0;
        public const int SaltIndex = 1;
        public const int HashIndex = 2;

        public string Hash(string input)
        {
            // Generate a random salt
            var rnd = new RNGCryptoServiceProvider();
            var salt = new byte[SaltLength];
            rnd.GetBytes(salt);

            // Hash the password and encode the parameters
            var hash = Hash(input, salt, HashIterations, HashLength);
            var result = string.Format("{0}:{1}:{2}", HashIterations, Convert.ToBase64String(salt), Convert.ToBase64String(hash));

            return result;
        }

        public string GetNonce(int size = 32)
        {
            var crypto = new RNGCryptoServiceProvider();
            var buffer = new byte[size];
            crypto.GetBytes(buffer);
            return Convert.ToBase64String(buffer);
        }

        public bool ValidatePassword(string password, string hash)
        {
            char[] delimiter = { ':' };
            var split = hash.Split(delimiter);
            var iterations = Int32.Parse(split[IterationIndex]);
            var salt = Convert.FromBase64String(split[SaltIndex]);
            var hashBytes = Convert.FromBase64String(split[HashIndex]);
            var testHash = Hash(password, salt, iterations, hashBytes.Length);
            return SlowEquals(hashBytes, testHash);
        }

        private static bool SlowEquals(IList<byte> left, IList<byte> right)
        {
            var diff = (uint)left.Count ^ (uint)right.Count;
            for (var i = 0; i < left.Count && i < right.Count; i++)
            {
                diff |= (uint)(left[i] ^ right[i]);
            }
            return diff == 0;
        }
        
        private static byte[] Hash(string input, byte[] salt, int iterations, int outputBytes)
        {
            var pbkdf2 = new Rfc2898DeriveBytes(input, salt) {IterationCount = iterations};
            return pbkdf2.GetBytes(outputBytes);
        }
    }
}