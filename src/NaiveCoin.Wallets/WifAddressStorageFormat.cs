using System;
using System.Diagnostics.Contracts;
using System.Linq;
using NaiveCoin.Core;

namespace NaiveCoin.Wallets
{
	/// <summary>
	/// WIF: https://en.bitcoin.it/wiki/Wallet_import_format
	/// </summary>
	public class WifAddressStorageFormat : IWalletAddressStorageFormat
	{
		public KeyPair Import(Wallet wallet, string wif)
		{
			// Take a Wallet Import Format string
			Contract.Assert(wallet != null);
			Contract.Assert(!string.IsNullOrWhiteSpace(wif));

			var privateKey = GetPrivateKeyFromImport(wif);

			var importedRaw = Ed25519.GenerateKeyPairFromPrivateKey(privateKey);

			var imported = new KeyPair(
				wallet.KeyPairs.Count + 1,
				importedRaw.Item1,
				importedRaw.Item2
			);

			wallet.KeyPairs.Add(imported);
			return imported;
		}

		public static byte[] GetPrivateKeyFromImport(string wif)
		{
			// Convert it to a byte string using Base58Check encoding
			var decoded = Base58Check.DecodePlain(wif);

			// Drop the last 4 checksum bytes from the byte string
			// Drop the first byte (it should be 0x80)
			// If the private key corresponded to a compressed public key, also drop the last byte (it should be 0x01)
			var privateKey = decoded.Take(decoded.Length - 4).Skip(1).Take(32).ToArray();

			return privateKey;
		}

		public string Export(Wallet wallet, byte[] publicKey)
		{
			Contract.Assert(wallet != null);
			Contract.Assert(publicKey != null && publicKey.Length > 0);

			// Take a private key
			var privateKey = wallet.GetPrivateKeyByAddress(publicKey);
			Contract.Assert(privateKey != null);

			// Add a 0x80 byte in front of it for mainnet addresses or 0xef for testnet addresses. 
			// Also add a 0x01 byte at the end if the private key will correspond to a compressed public key
			var header = new byte[] {0x80};
			var footer = new byte[] {0x01};
			var extendedKey = new byte[header.Length + privateKey.Length + footer.Length];
			Buffer.BlockCopy(header, 0, extendedKey, 0, header.Length);
			Buffer.BlockCopy(privateKey, 0, extendedKey, header.Length, privateKey.Length);
			Buffer.BlockCopy(footer, 0, extendedKey, header.Length + privateKey.Length, footer.Length);

			// Perform SHA-256 hash on the extended key
			// Perform SHA-256 hash on result of SHA-256 hash
			// Take the first 4 bytes of the second SHA-256 hash, this is the checksum
			byte[] checksum = extendedKey.Sha256().Sha256().Take(4).ToArray();

			// Add the 4 checksum bytes from point 5 at the end of the extended key from point 2
			byte[] exportKey = new byte[extendedKey.Length + checksum.Length];
			Buffer.BlockCopy(extendedKey, 0, exportKey, 0, extendedKey.Length);
			Buffer.BlockCopy(checksum, 0, exportKey, extendedKey.Length, checksum.Length);

			// Convert the result from a byte string into a base58 string using Base58Check encoding
			return Base58Check.EncodePlain(exportKey);
		}
	}
}