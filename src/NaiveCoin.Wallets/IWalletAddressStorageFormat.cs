using NaiveCoin.Core.Models;

namespace NaiveCoin.Wallets
{
    public interface IWalletAddressStorageFormat
    {
        KeyPair Import(Wallet wallet, string input);
        string Export(Wallet wallet, string address);
    }
}