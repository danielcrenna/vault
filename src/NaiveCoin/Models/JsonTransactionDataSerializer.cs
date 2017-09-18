using System.Text;
using Newtonsoft.Json;

namespace NaiveCoin.Models
{
    public class JsonTransactionDataSerializer : ITransactionDataSerializer
    {
        private readonly JsonSerializerSettings _jsonSettings;

        public JsonTransactionDataSerializer(JsonSerializerSettings jsonSettings)
        {
            _jsonSettings = jsonSettings;
        }

        public byte[] Serialize(TransactionData transactionData)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(transactionData, _jsonSettings));
        }

        public TransactionData Deserialize(byte[] json)
        {
            return JsonConvert.DeserializeObject<TransactionData>(Encoding.UTF8.GetString(json));
        }
    }
}