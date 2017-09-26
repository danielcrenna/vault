using NaiveChain;

namespace NaiveCoin.Tests.Fixtures
{
    public class BlockObjectDataSerializerFixture
    {
        public BlockObjectDataSerializerFixture()
        {
            var settings = new JsonSerializerSettingsFixture().Value;

            Value = new JsonBlockObjectSerializer(settings);
        }

        public JsonBlockObjectSerializer Value { get; set; }
    }
}