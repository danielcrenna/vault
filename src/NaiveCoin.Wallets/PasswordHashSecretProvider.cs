using System.Diagnostics.Contracts;
using NaiveCoin.Core.Helpers;

namespace NaiveCoin.Wallets
{
	/// <summary>
	/// Uses the wallet's password hash to generate a secret.
	/// Necessary for any wallets that need determinism; should be used with care to avoid
	/// lazy passwords causing loss of funds.
	/// </summary>
    public class PasswordHashSecretProvider : IWalletSecretProvider
    {
        public byte[] GenerateSecret(Wallet wallet)
        {
            Contract.Assert(!string.IsNullOrWhiteSpace(wallet.PasswordHash));
	        wallet.Secret = PasswordUtil.FastHash(wallet.PasswordHash);
			return wallet.Secret;
        }
    }
}