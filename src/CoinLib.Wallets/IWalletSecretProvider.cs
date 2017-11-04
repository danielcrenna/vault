namespace CoinLib.Wallets
{
    public interface IWalletSecretProvider
    {
        byte[] GenerateSecret(Wallet wallet);
    }
}