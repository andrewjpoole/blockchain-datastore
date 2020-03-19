using DataStore.Contracts;

namespace DataStore.BlockchainDB
{
    public class ItemMetadata : IItemMetadata
    {
        public string BlobRef { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}