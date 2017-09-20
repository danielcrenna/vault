using System.Collections.Generic;

namespace NaiveCoin.Wallets
{
    public interface IWalletRepository
    {
        IEnumerable<Wallet> GetAll();
        Wallet GetById(string id);

        Wallet CreateFromPassword(string password);
        Wallet CreateFromPasswordHash(string passwordHash);
        Wallet Add(Wallet wallet);

        void SaveAddresses(Wallet wallet);
    }
}