namespace NaiveCoin.Wallets
{
    public interface IWalletSecretProvider
    {
        byte[] GenerateSecret(Wallet wallet);
    }
}