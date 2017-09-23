using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace NaiveCoin.Core.Helpers
{
    /// <summary>
    /// Source: https://github.com/defuse/password-hashing/blob/master/PasswordStorage.cs
    /// </summary>
    internal class Pbkdf2
    {
        class InvalidHashException : Exception
        {
            public InvalidHashException() { }
            public InvalidHashException(string message)
                : base(message) { }
            public InvalidHashException(string message, Exception inner)
                : base(message, inner) { }
        }

        class CannotPerformOperationException : Exception
        {
            public CannotPerformOperationException() { }
            public CannotPerformOperationException(string message)
                : base(message) { }
            public CannotPerformOperationException(string message, Exception inner)
                : base(message, inner) { }
        }

        // These constants may be changed without breaking existing hashes.
        public const int SALT_BYTES = 24;
        public const int HASH_BYTES = 18;
        public const int PBKDF2_ITERATIONS = 64000;

        // These constants define the encoding and may not be changed.
        public const int HASH_SECTIONS = 5;
        public const int HASH_ALGORITHM_INDEX = 0;
        public const int ITERATION_INDEX = 1;
        public const int HASH_SIZE_INDEX = 2;
        public const int SALT_INDEX = 3;
        public const int PBKDF2_INDEX = 4;

        public static byte[] CreateRawHash(string password, string salt, int iterations, int hashBytes, HashAlgorithmName algorithmName)
        {
            byte[] saltBytes = Encoding.UTF8.GetBytes(salt);
            byte[] hash = DeriveKey(password, saltBytes, PBKDF2_ITERATIONS, hashBytes, algorithmName);
            return hash;
        }

        public static string CreateStorageHash(string password, int saltBytes = SALT_BYTES, int iterations = PBKDF2_ITERATIONS, int hashBytes = HASH_BYTES, HashAlgorithmName? algorithmName = null)
        {
            // Generate a random salt
            byte[] salt = new byte[saltBytes];
            try
            {
                using (RNGCryptoServiceProvider csprng = new RNGCryptoServiceProvider())
                {
                    csprng.GetBytes(salt);
                }
            }
            catch (CryptographicException ex)
            {
                throw new CannotPerformOperationException(
                    "Random number generator not available.",
                    ex
                );
            }
            catch (ArgumentNullException ex)
            {
                throw new CannotPerformOperationException(
                    "Invalid argument given to random number generator.",
                    ex
                );
            }

            byte[] hash = DeriveKey(password, salt, iterations, hashBytes, algorithmName ?? HashAlgorithmName.SHA256);

            // format = algorithmName:iterations:hashSize:salt:hash
            var parts = $"{algorithmName}:{iterations}:{hash.Length}:{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";

            return parts;
        }

        public static bool VerifyPassword(string password, string goodHash)
        {
            char[] delimiter = { ':' };
            string[] split = goodHash.Split(delimiter);

            if (split.Length != HASH_SECTIONS)
            {
                throw new InvalidHashException(
                    "Fields are missing from the password hash."
                );
            }
            
            int iterations;
            try
            {
                iterations = int.Parse(split[ITERATION_INDEX]);
            }
            catch (ArgumentNullException ex)
            {
                throw new CannotPerformOperationException(
                    "Invalid argument given to Int32.Parse",
                    ex
                );
            }
            catch (FormatException ex)
            {
                throw new InvalidHashException(
                    "Could not parse the iteration count as an integer.",
                    ex
                );
            }
            catch (OverflowException ex)
            {
                throw new InvalidHashException(
                    "The iteration count is too large to be represented.",
                    ex
                );
            }

            if (iterations < 1)
            {
                throw new InvalidHashException(
                    "Invalid number of iterations. Must be >= 1."
                );
            }

            byte[] salt;
            try
            {
                salt = Convert.FromBase64String(split[SALT_INDEX]);
            }
            catch (ArgumentNullException ex)
            {
                throw new CannotPerformOperationException(
                    "Invalid argument given to Convert.FromBase64String",
                    ex
                );
            }
            catch (FormatException ex)
            {
                throw new InvalidHashException(
                    "Base64 decoding of salt failed.",
                    ex
                );
            }

            byte[] hash;
            try
            {
                hash = Convert.FromBase64String(split[PBKDF2_INDEX]);
            }
            catch (ArgumentNullException ex)
            {
                throw new CannotPerformOperationException(
                    "Invalid argument given to Convert.FromBase64String",
                    ex
                );
            }
            catch (FormatException ex)
            {
                throw new InvalidHashException(
                    "Base64 decoding of pbkdf2 output failed.",
                    ex
                );
            }

            int storedHashSize;
            try
            {
                storedHashSize = int.Parse(split[HASH_SIZE_INDEX]);
            }
            catch (ArgumentNullException ex)
            {
                throw new CannotPerformOperationException(
                    "Invalid argument given to Int32.Parse",
                    ex
                );
            }
            catch (FormatException ex)
            {
                throw new InvalidHashException(
                    "Could not parse the hash size as an integer.",
                    ex
                );
            }
            catch (OverflowException ex)
            {
                throw new InvalidHashException(
                    "The hash size is too large to be represented.",
                    ex
                );
            }

            if (storedHashSize != hash.Length)
            {
                throw new InvalidHashException(
                    "HashDigest length doesn't match stored hash length."
                );
            }

            HashAlgorithmName algorithm;
            try
            {
                algorithm = new HashAlgorithmName(split[HASH_ALGORITHM_INDEX]);
            }
            catch (Exception ex)
            {
                throw new InvalidHashException(
                    "The hash algorithm is invalid.",
                    ex
                );
            }

            byte[] testHash = DeriveKey(password, salt, iterations, hash.Length, algorithm);

            return SlowEquals(hash, testHash);
        }

        public static bool SlowEquals(byte[] a, byte[] b)
        {
            uint diff = (uint)a.Length ^ (uint)b.Length;
            for (int i = 0; i < a.Length && i < b.Length; i++)
            {
                diff |= (uint)(a[i] ^ b[i]);
            }
            return diff == 0;
        }

        public static byte[] DeriveKey(string password, byte[] salt, int iterations, int outputBytes, HashAlgorithmName hashAlgorithmName)
        {
            KeyDerivationPrf algorithm;
            if (hashAlgorithmName == HashAlgorithmName.SHA1)
                algorithm = KeyDerivationPrf.HMACSHA1;
            else if(hashAlgorithmName == HashAlgorithmName.SHA256)
                algorithm = KeyDerivationPrf.HMACSHA256;
            else if (hashAlgorithmName == HashAlgorithmName.SHA512)
                algorithm = KeyDerivationPrf.HMACSHA512;
            else
             throw new NotSupportedException();

            return KeyDerivation.Pbkdf2(password, salt, algorithm, iterations, outputBytes);
        }
    }
}