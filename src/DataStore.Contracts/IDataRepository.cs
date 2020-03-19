using System.Collections.Generic;
using System.IO;
using DataStore.LiteDB;

namespace DataStore.Contracts
{
    public interface IDataRepository
    {
        // TODO: split this interface

        IEnumerable<IItemMetadata> GetMetadata(string blobRef);
        IEnumerable<IItemMetadata> GetMetadata(string blobRef, string Name);
        void PostMetadata(string blobRef, string name, string value);
        int DeleteMetadata(string blobId, string name);
        int DeleteMetadata(string blobId);

        IEnumerable<IBlobFileInfo> GetAllBlobReferences(int itemsPerPage, int page);
        IEnumerable<string> FindBlobIdsViaMetadata(string name, string value, bool exactMatch);
        int GetCountOfAllBlobReferences();

        IBlobFileInfo GetBlob(string blobRef, Stream output);
        bool BlobExists(string blobRef);
        (string BlobId, bool Created) PostBlob(Stream blob, string fileName, string hash);
        bool DeleteBlob(string blobRef);
    }
}
