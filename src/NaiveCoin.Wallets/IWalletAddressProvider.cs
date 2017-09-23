namespace NaiveCoin.Wallets
{
    public interface IWalletAddressProvider
    {
        string GenerateAddress(Wallet wallet);
    }
}