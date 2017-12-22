namespace ChainLib.Wallets
{
    public interface IWalletSecretProvider
    {
        byte[] GenerateSecret(params object[] args);
    }
}