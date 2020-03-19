using Newtonsoft.Json;

namespace DataStore.Blockchain
{
    public class Transaction
    {
        private readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings{ TypeNameHandling = TypeNameHandling.All };

        public readonly string FromNode;
        public readonly string Mode;
        public readonly string Data;

        [JsonConstructor]
        public Transaction(string fromNode, string mode, string data)
        {
            FromNode = fromNode;
            Mode = mode;
            Data = data;
        }

        public Transaction(string fromNode, string mode, object data)
        {
            FromNode = fromNode;
            Mode = mode;
            
            Data = JsonConvert.SerializeObject(data, jsonSerializerSettings);
        }

        public T GetData<T>()
        {
            var deserialisedInstruction = JsonConvert.DeserializeObject<MetadataInstruction>(Data, jsonSerializerSettings);
            var deserialisedT = JsonConvert.DeserializeObject<T>(deserialisedInstruction.Data, jsonSerializerSettings);
            return deserialisedT;
        }

        public override string ToString()
        {
            return $"{Data}";
        }

        public MetadataInstruction GetMetadataInstruction()
        {
            var deserialisedInstruction = JsonConvert.DeserializeObject<MetadataInstruction>(Data, jsonSerializerSettings);
            return deserialisedInstruction;
        }
    }
}
