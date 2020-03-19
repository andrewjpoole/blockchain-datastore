namespace DataStore.Contracts
{
    public interface IItemMetadata
    {
        //string Id { get; set; }
        string BlobRef { get; set; }
        string Name { get; set; }
        string Value { get; set; }
    }
}