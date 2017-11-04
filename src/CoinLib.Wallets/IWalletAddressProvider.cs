namespace CoinLib.Wallets
{
    public interface IWalletAddressProvider
    {
        string GenerateAddress(Wallet wallet);
    }
}