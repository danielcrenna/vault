namespace NaiveCoin.Core.Tests.Fixtures
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