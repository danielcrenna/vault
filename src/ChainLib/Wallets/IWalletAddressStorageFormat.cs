using ChainLib.Crypto;

namespace ChainLib.Wallets
{
    public interface IWalletAddressStorageFormat
    {
        KeyPair Import(Wallet wallet, string input, int len);
        string Export(Wallet wallet, byte[] address);
    }
}