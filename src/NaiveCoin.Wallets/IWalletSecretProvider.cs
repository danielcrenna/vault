namespace NaiveCoin.Wallets
{
    public interface IWalletSecretProvider
    {
        string GenerateSecret(Wallet wallet);
    }
}