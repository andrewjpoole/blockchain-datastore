namespace DataStore.ConfigOptions
{
    public class NodeAppSettingsOptions
    {
        public string TempFileDirectory { get; set; }
        public string SecurityKey { get; set; }
        public string LiteDbDataLocation { get; set; }
        public string AzureBlobStoreConnectionString { get; set; }
        public string AzureBlobContainerReference { get; set; }
    }
}