namespace ChainLib.Wallets
{
    public interface IWalletAddressProvider
    {
        string GenerateAddress(Wallet wallet);
    }
}