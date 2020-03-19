using Newtonsoft.Json;

namespace DataStore.Blockchain
{
    public class MetadataInstruction
    {
        private readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };

        public Actions Action { get; set; }
        public string Data { get; set; }

        public T GetData<T>()
        {
            var deserialisedData = JsonConvert.DeserializeObject<T>(Data, jsonSerializerSettings);
            return deserialisedData;
        }
    }

    public enum Actions
    {
        Add,
        Remove
    }
}