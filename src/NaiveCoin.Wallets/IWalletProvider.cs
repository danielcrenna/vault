namespace NaiveCoin.Wallets
{
    public interface IWalletProvider
    {
        string GenerateAddress(Wallet wallet);
        string GenerateSecret(Wallet wallet);
        Wallet CreateFromPassword(string password);
        Wallet CreateFromPasswordHash(string passwordHash);
    }
}