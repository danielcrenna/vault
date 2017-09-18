using NaiveCoin.Models;

namespace NaiveCoin.Tests.Fixtures
{
    public class TransactionDataSerializerFixture
    {
        public TransactionDataSerializerFixture()
        {
            var settings = new JsonSerializerSettingsFixture().Value;

            Value = new JsonTransactionDataSerializer(settings);
        }

        public JsonTransactionDataSerializer Value { get; set; }
    }
}