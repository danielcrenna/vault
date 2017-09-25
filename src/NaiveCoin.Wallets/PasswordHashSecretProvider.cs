using NaiveCoin.Core.Helpers;
using System.Diagnostics.Contracts;

namespace NaiveCoin.Wallets
{
    public class PasswordHashSecretProvider : IWalletSecretProvider
    {
        public byte[] GenerateSecret(Wallet wallet)
        {
            Contract.Assert(!string.IsNullOrWhiteSpace(wallet.PasswordHash));
            wallet.Secret = CryptoEdDsaUtil.GenerateSecret(wallet.PasswordHash);
            return wallet.Secret;
        }
    }
}