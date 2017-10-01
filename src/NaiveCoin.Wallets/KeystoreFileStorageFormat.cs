using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using NaiveCoin.Core.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Sodium;
using KeyPair = NaiveCoin.Core.Models.KeyPair;

namespace NaiveCoin.Wallets
{
	/// <summary>
	/// Stores a wallet address similar to offline wallet generators like MyEtherWallet.
	/// The output is a JSON file containing the encrypted private key, where the private key
	/// is sourced from the wallet's password hash (normally a new plaintext password, but our
	/// wallet already has a password, simplifying the process).
	/// 
	/// Normally, in these systems, the wallet is one-to-one with a single address.
	/// 
	/// Based on: https://github.com/MyEtherWallet/MyEtherWallet/blob/develop/common/libs/keystore.ts
	/// </summary>
	public class KeystoreFileStorageFormat : IWalletAddressStorageFormat
	{
		public const int KeystoreVersion = 1;

		public enum KdfType { Scrypt, Argon }

		private readonly KdfType _kdfType;
		private readonly JsonSerializerSettings _jsonSettings;

		public KeystoreFileStorageFormat(KdfType kdfType = KdfType.Scrypt)
		{
			_kdfType = kdfType;
			_jsonSettings = GetJsonSerializerSettings();
		}

		private static JsonSerializerSettings GetJsonSerializerSettings()
		{
			return new JsonSerializerSettings
			{
				DefaultValueHandling = DefaultValueHandling.Ignore,
				NullValueHandling = NullValueHandling.Ignore,
				Converters = new List<JsonConverter>
				{
					new StringEnumConverter(true)
				}
			};
		}

		public KeyPair Import(Wallet wallet, string input)
		{
			return ImportFromKeystore(wallet, input, _jsonSettings);
		}

		public string Export(Wallet wallet, byte[] address)
		{
			return ExportToKeystore(_kdfType, wallet, address, _jsonSettings);
		}

		public static string WriteToFile(string fileDirectory, Wallet wallet)
		{
			if (wallet?.KeyPairs == null || wallet.KeyPairs.Count != 1)
				throw new NotSupportedException("This function expects a wallet with a single address");

			FileAttributes attributes = File.GetAttributes(fileDirectory);
			if (!attributes.HasFlag(FileAttributes.Directory))
				throw new InvalidOperationException("The provided path must be a directory");
			if (!Directory.Exists(fileDirectory))
				throw new InvalidOperationException("The provided directory path does not exist");
			
			var address = wallet.KeyPairs[0].PublicKey;
			var json = ExportToKeystore(KdfType.Scrypt, wallet, address, GetJsonSerializerSettings());

			// UTC--2017-10-01T17-22-26.196Z--01caf1a3bf2164ec410b888c74d82a292c326487
			var timestamp = $"{DateTimeOffset.UtcNow:s}".Replace(':', '-');
			var lastDash = timestamp.LastIndexOf('-');
			timestamp = timestamp.Remove(lastDash, 1);
			timestamp = timestamp.Insert(lastDash, ".");
			var filename = Path.Combine(fileDirectory, $"UTC--{timestamp}Z--{address.ToHex()}");
			File.WriteAllText(filename, json);

			return filename;
		}

		public static KeyPair ImportFromKeystore(Wallet wallet, string input, JsonSerializerSettings jsonSettings)
		{
			Contract.Assert(!string.IsNullOrWhiteSpace(wallet.PasswordHash));
			var passwordHashBytes = Encoding.UTF8.GetBytes(wallet.PasswordHash);

			var kstore = JsonConvert.DeserializeObject<Keystore>(input, jsonSettings);
			if (kstore.Version != KeystoreVersion)
				throw new NotSupportedException("Unsupported version");

			string kdfTypeString = kstore.Crypto.Kdf;
			if (!Enum.TryParse(kdfTypeString, true, out KdfType kdfType))
				throw new NotSupportedException("Unsupported kdf");

			byte[] derivedKey;
			switch (kdfType)
			{
				case KdfType.Scrypt:
					{
						derivedKey = PasswordHash.ScryptHashBinary(
							passwordHashBytes,
							kstore.Crypto.KdfParameters.Salt.FromHex(),
							PasswordHash.Strength.Sensitive,
							kstore.Crypto.KdfParameters.DerivedKeyLength);
						break;
					}
				case KdfType.Argon:
					{
						derivedKey = PasswordHash.ArgonHashBinary(
							passwordHashBytes,
							kstore.Crypto.KdfParameters.Salt.FromHex(),
							PasswordHash.StrengthArgon.Sensitive,
							kstore.Crypto.KdfParameters.DerivedKeyLength);
						break;
					}
				default:
					throw new NotSupportedException("Unsupported kdf");
			}

			string ciphertext = kstore.Crypto.CipherText;
			string mac = kstore.Crypto.Mac;
			string iv = kstore.Crypto.CipherParameters.Iv;

			byte[] nonce = derivedKey.Take(24).ToArray();
			byte[] seed = SecretBox.OpenDetached(ciphertext, mac.FromHex(), nonce, iv.FromHex());

			var imported = new KeyPair(
				wallet.KeyPairs.Count + 1,
				kstore.Address.FromHex(),
				seed);

			wallet.KeyPairs.Add(imported);
			return imported;
		}

		public static string ExportToKeystore(KdfType kdfType, Wallet wallet, byte[] address, JsonSerializerSettings jsonSettings)
		{
			Contract.Assert(!string.IsNullOrWhiteSpace(wallet.PasswordHash));
			var passwordHashBytes = Encoding.UTF8.GetBytes(wallet.PasswordHash);

			var salt = CryptoUtil.RandomBytes(32);
			var kdfparams = new
			{
				dklen = 32,
				salt = salt.ToHex()
			};

			byte[] derivedKey;
			switch (kdfType)
			{
				case KdfType.Scrypt:
					{
						derivedKey = PasswordHash.ScryptHashBinary(passwordHashBytes, salt, PasswordHash.Strength.Sensitive, kdfparams.dklen);
						break;
					}
				case KdfType.Argon:
					{
						derivedKey = PasswordHash.ArgonHashBinary(passwordHashBytes, salt, PasswordHash.StrengthArgon.Sensitive, kdfparams.dklen);
						break;
					}
				default:
					throw new NotSupportedException("Unsupported kdf");
			}

			var iv = CryptoUtil.RandomBytes(32);
			var pKey = wallet.GetPrivateKeyByAddress(address);
			var nonce = derivedKey.Take(24).ToArray();
			var box = SecretBox.CreateDetached(pKey, nonce, iv);

			var keystore = new Keystore
			{
				Version = KeystoreVersion,
				Id = wallet.Id,
				Address = address.ToHex(),
				Crypto = new KeystoreCrypto
				{
					CipherText = box.CipherText.ToHex(),
					CipherParameters = new KeystoreCryptoParameters
					{
						Iv = iv.ToHex()
					},
					Cipher = "crypto_secretbox_detached",
					Kdf = kdfType.ToString().ToLowerInvariant(),
					KdfParameters = new KeystoreKdfParameters
					{
						Salt = kdfparams.salt,
						DerivedKeyLength = kdfparams.dklen,
					},
					Mac = box.Mac.ToHex()
				}
			};

			return JsonConvert.SerializeObject(keystore, Formatting.None, jsonSettings);
		}

		#region JSON Schema

		public class Keystore
		{
			[JsonProperty("version")]
			public int Version { get; set; }

			[JsonProperty("id")]
			public string Id { get; set; }

			[JsonProperty("address")]
			public string Address { get; set; }

			[JsonProperty("Crypto")]
			public KeystoreCrypto Crypto { get; set; }
		}


		public class KeystoreCrypto
		{
			[JsonProperty("ciphertext")]
			public string CipherText { get; set; }

			[JsonProperty("cipherparams")]
			public KeystoreCryptoParameters CipherParameters { get; set; }

			[JsonProperty("cipher")]
			public string Cipher { get; set; }

			[JsonProperty("kdf")]
			public string Kdf { get; set; }

			[JsonProperty("kdfparams")]
			public KeystoreKdfParameters KdfParameters { get; set; }

			[JsonProperty("mac")]
			public string Mac { get; set; }
		}

		public class KeystoreCryptoParameters
		{
			[JsonProperty("iv")]
			public string Iv { get; set; }
		}

		public class KeystoreKdfParameters
		{
			[JsonProperty("dklen")]
			public int DerivedKeyLength { get; set; }

			[JsonProperty("salt")]
			public string Salt { get; set; }
		}

		#endregion
	}
}