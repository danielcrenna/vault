using NaiveCoin.Models;
using NaiveCoin.Wallets;

namespace NaiveCoin.Tests.Fixtures
{
    public class WalletProviderFixture
    {
        public WalletProviderFixture()
        {
            Value = new BrainWalletProvider();
        }

        public IWalletProvider Value { get; set; }
    }
}