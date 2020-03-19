using System.IO;

namespace DataStore.BlockchainDB
{
    public interface IAzureBlobStore
    {
        void Connect();
        void UploadFile(string blobRef, Stream fileStream);
        Stream DownloadFile(string blobRef);
        void DeleteFile(string blobRef);
        bool FileExists(string blobRef);
    }
}