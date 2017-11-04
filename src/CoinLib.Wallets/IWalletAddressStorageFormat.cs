using Crypto.Shim;

namespace CoinLib.Wallets
{
    public interface IWalletAddressStorageFormat
    {
        KeyPair Import(Wallet wallet, string input);
        string Export(Wallet wallet, byte[] address);
    }
}