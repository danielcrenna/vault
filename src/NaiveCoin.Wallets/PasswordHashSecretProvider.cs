using NaiveCoin.Core.Helpers;
using System.Diagnostics.Contracts;

namespace NaiveCoin.Wallets
{
    public class PasswordHashSecretProvider : IWalletSecretProvider
    {
        public string GenerateSecret(Wallet wallet)
        {
            // Assumption: the password hash for the wallet is not salted
            Contract.Assert(!string.IsNullOrWhiteSpace(wallet.PasswordHash));
            wallet.Secret = CryptoEdDsaUtil.GenerateSecret(wallet.PasswordHash);
            return wallet.Secret;
        }
    }
}