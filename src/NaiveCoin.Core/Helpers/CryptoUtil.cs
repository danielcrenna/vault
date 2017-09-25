using System;
using System.Diagnostics.Contracts;
using System.Security.Cryptography;
using System.Text;
using Sodium;

namespace NaiveCoin.Core.Helpers
{
    public static class CryptoUtil
    {
        private static readonly RNGCryptoServiceProvider Random = new RNGCryptoServiceProvider();

        public static string RandomString(int size = 64)
        {
            var random = new byte[(int)Math.Floor(size / 2m)];
            Random.GetNonZeroBytes(random);
            return ToHex(random);
        }

        public static string ToHex(this byte[] input)
        {
	        return Utilities.BinaryToHex(input);
        }

        public static byte[] FromHex(this string input)
        {
	        return Utilities.HexToBinary(input);
        }

        public static byte[] Sha256(this byte[] input)
        {
            using (var algorithm = SHA256.Create())
                return algorithm.ComputeHash(input);
        }

        public static byte[] Sha256(this string input)
        {
            return Sha256(Encoding.UTF8.GetBytes(input));
        }

	    public static bool SlowEquals(byte[] a, byte[] b)
	    {
		    return Utilities.Compare(a, b); // WARNING: I am assuming this is constant time!
	    }

		/// <summary>
		/// Produces a password hash suitable for long term storage. This means using a random salt per password, high entropy, and
		/// high number of key stretching operations.
		/// 
		/// It's important to distinguish this from a Wallet address' private key.
		/// Normally, unless you're creating a "brain wallet", this should never be used as the seed for a private key, since
		/// remembering the password is the only thing necessary to derive a private key.
		/// </summary>
		/// <param name="password"></param>
		/// <param name="salt"></param>
		/// <returns></returns>
		public static string HashPassword(string password, string salt = null)
        {
	        Contract.Assert(!string.IsNullOrWhiteSpace(password));
			var saltBytes = ArgonSalt(salt);
			var hashBytes = ArgonHash(password, saltBytes);
	        return $"{Convert.ToBase64String(saltBytes)}:{Convert.ToBase64String(hashBytes)}";
        }

	    public static bool VerifyPassword(string password, string passwordHash)
		{
			var tokens = passwordHash.Split(new[] {':'}, StringSplitOptions.RemoveEmptyEntries);
			var saltBytes = Convert.FromBase64String(tokens[0]);
			var compareHashBytes = Convert.FromBase64String(tokens[1]);
			var hashBytes = ArgonHash(password, saltBytes);
			return SlowEquals(compareHashBytes, hashBytes);
		}

		private static byte[] ArgonSalt(string salt)
	    {
		    return salt == null ? PasswordHash.ArgonGenerateSalt() : Encoding.UTF8.GetBytes(salt);
	    }

	    private static byte[] ArgonHash(string password, byte[] saltBytes)
	    {
		    var hashBytes = PasswordHash.ArgonHashBinary(
			    Encoding.UTF8.GetBytes(password),
			    saltBytes,
			    PasswordHash.StrengthArgon.Sensitive, 512);
		    return hashBytes;
	    }
	}
}