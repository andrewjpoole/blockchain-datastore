using DataStore.Contracts;

namespace DataStore.Models
{
    public class ItemMetadata : IItemMetadata
    {
        public string Id { get; set; }
        public string BlobRef { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
