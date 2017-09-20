using NaiveCoin.Core.Helpers;

namespace NaiveCoin.Tests.Fixtures
{
    public class RandomStringFixture
    {
        public RandomStringFixture()
        {
            Value = CryptoUtil.RandomString();
        }

        public string Value { get; set; }
    }
}