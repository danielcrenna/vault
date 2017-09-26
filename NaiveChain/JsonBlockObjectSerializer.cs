using System.Text;
using Newtonsoft.Json;

namespace NaiveChain
{
    public class JsonBlockObjectSerializer : IBlockObjectSerializer
    {
        private readonly JsonSerializerSettings _jsonSettings;

        public JsonBlockObjectSerializer(JsonSerializerSettings jsonSettings)
        {
            _jsonSettings = jsonSettings;
        }

        public byte[] Serialize(BlockObject transactionData)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(transactionData, _jsonSettings));
        }

        public BlockObject Deserialize(byte[] json)
        {
            return JsonConvert.DeserializeObject<BlockObject>(Encoding.UTF8.GetString(json));
        }
    }
}