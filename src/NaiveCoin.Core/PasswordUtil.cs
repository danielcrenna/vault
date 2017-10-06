using System;
using System.Diagnostics.Contracts;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Sodium;

namespace NaiveCoin.Core
{
	public static class PasswordUtil
	{
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
		public static string StorageHash(string password, string salt = null)
		{
			Contract.Assert(!string.IsNullOrWhiteSpace(password));
			var saltBytes = ArgonSalt(salt);
			var hashBytes = ArgonHash(password, saltBytes);
			return $"{Convert.ToBase64String(saltBytes)}:{Convert.ToBase64String(hashBytes)}";
		}

		/// <summary>
		/// Produces a password hash with a fixed salt in reasonably fast time.
		/// Used when we need a quasi-secure hash that is backed by something else
		/// more secure (i.e.: brain wallets where we use the argon hash as the password)
		/// </summary>
		/// <param name="password"></param>
		/// <returns></returns>
		public static byte[] FastHash(string password)
		{
			return KeyDerivation.Pbkdf2(password, Encoding.UTF8.GetBytes("salt"), KeyDerivationPrf.HMACSHA512, 64000, 32);
		}

		public static bool Verify(string password, string passwordHash)
		{
			var tokens = passwordHash.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
			var saltBytes = Convert.FromBase64String(tokens[0]);
			var compareHashBytes = Convert.FromBase64String(tokens[1]);
			var hashBytes = ArgonHash(password, saltBytes);
			return CryptoUtil.SlowEquals(compareHashBytes, hashBytes);
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