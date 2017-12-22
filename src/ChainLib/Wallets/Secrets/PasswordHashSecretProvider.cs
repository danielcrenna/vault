using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.InteropServices;
using Crypto.Shim;

namespace ChainLib.Wallets.Secrets
{
	/// <summary>
	/// Uses the wallet's password hash to generate a secret.
	/// Necessary for any wallets that need determinism; should be used with care to avoid lazy passwords causing loss of funds.
	/// </summary>
    public class PasswordHashSecretProvider : IWalletSecretProvider
    {
        public byte[] GenerateSecret(params object[] args)
	    {
		    var wallet = args.FirstOrDefault() as Wallet;
			Contract.Assert(wallet != null);
		    Contract.Assert(!string.IsNullOrWhiteSpace(wallet.PasswordHash));
		    wallet.Secret = PasswordUtil.FastHash(wallet.PasswordHash, "salt");
		    return wallet.Secret;
		}
    }
}