using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DataStore.ConfigOptions;
using DataStore.Contracts;
using DataStore.LiteDB;
using DataStore.Models;
using DataStore.P2P;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;

namespace DataStore.BlockchainDB
{
    public class BlockchainDBRepository : IDataRepository
    {
        private readonly ILogger _logger;
        private readonly IAggregateState<ItemMetadata> _aggregateState;
        private readonly IAzureBlobStore _azureBlobStore;
        private readonly INode _p2PNode;
        private readonly NodeAppSettingsOptions _nodeAppSettingsOptions;

        private const string BlobRef_Idx = "BlobRef";
        private const string Metadata_Name_Idx = "Name";
        private const string Metadata_Value_Idx = "Value";

        public BlockchainDBRepository(
            ILogger logger,
            IAggregateState<ItemMetadata> aggregateState,
            IOptions<NodeAppSettingsOptions> nodeAppSettingsOptions,
            IOptions<NodeP2PSettingsOptions> nodeP2POptions,
            IAzureBlobStore azureBlobStore
            )
        {
            _logger = logger;
            _nodeAppSettingsOptions = nodeAppSettingsOptions.Value;
            
            _aggregateState = aggregateState;
            _aggregateState.PersistChain = true;

            _azureBlobStore = azureBlobStore;
            _azureBlobStore.Connect();

            var nodeAddress = new NodeAddress("127.0.0.1", nodeP2POptions.Value.Port);
            var addressList = new List<NodeAddress>();
            _p2PNode = new Node(nodeAddress, addressList);

            // wire up P2P behaviours
            _aggregateState.WireUpNode(_p2PNode);

            // setup indices
            _aggregateState.AddIndex(BlobRef_Idx, (item) => item.BlobRef);
            _aggregateState.AddIndex(Metadata_Name_Idx, (item) => item.Name);
            _aggregateState.AddIndex(Metadata_Value_Idx, (item) => item.Value);

            _aggregateState.InitialiseChainFromBlobs();
        }
        
        public IEnumerable<IItemMetadata> GetMetadata(string blobRef)
        {
            var matches = _aggregateState.FindBySingleIndex(new IndexSearch(BlobRef_Idx, blobRef));
            return matches;
        }

        public IEnumerable<IItemMetadata> GetMetadata(string blobRef, string metadataName)
        {
            var matches = _aggregateState.FindByIntersection(new IndexSearch(BlobRef_Idx, blobRef), new IndexSearch(Metadata_Name_Idx, metadataName));
            return matches;
        }
        public IEnumerable<string> FindBlobIdsViaMetadata(string name, string value, bool exactMatch)
        {
            var matches = _aggregateState.FindByIndexEvaluateValues(new IndexSearch(Metadata_Name_Idx, name),
                item => item.Value.Contains(value));

            return matches.Select(x => x.BlobRef);
        }

        public void PostMetadata(string blobRef, string name, string value)
        {
            var newItem = new ItemMetadata
            {
                BlobRef = blobRef,
                Name = name,
                Value = value
            };
            _aggregateState.LocalAdd(newItem);
        }

        public int DeleteMetadata(string blobId, string name)
        {
            var itemsToRemove = GetMetadata(blobId, name);
            var deleted = 0;
            foreach (var itemToRemove in itemsToRemove)
            {
                _aggregateState.LocalRemove((ItemMetadata)itemToRemove);
                deleted++;
            }

            return deleted;
        }

        public int DeleteMetadata(string blobId)
        {
            var itemsToRemove = GetMetadata(blobId);
            var deleted = 0;
            foreach (var itemToRemove in itemsToRemove)
            {
                _aggregateState.LocalRemove((ItemMetadata)itemToRemove);
                deleted++;
            }

            return deleted;
        }

        public IEnumerable<IBlobFileInfo> GetAllBlobReferences(int itemsPerPage, int page)
        {
            return _aggregateState.FindBySingleIndex(new IndexSearch(Metadata_Name_Idx, "FileReference"))
                .Skip(itemsPerPage * page)
                .Take(itemsPerPage).Select(x => JsonConvert.DeserializeObject<BlobFileInfo>(x.Value));
        }

        public int GetCountOfAllBlobReferences()
        {
            return _aggregateState.FindBySingleIndex(new IndexSearch(Metadata_Name_Idx, "FileReference")).Count();
        }

        public IBlobFileInfo GetBlob(string blobRef, Stream output)
        {
            var stream = _azureBlobStore.DownloadFile(blobRef);
            stream.CopyTo(output);

            return GetBlobFileInfo(blobRef);
        }

        private IBlobFileInfo GetBlobFileInfo(string blobRef)
        {
            var fileReferenceMetadata = _aggregateState.FindByIntersection(
                new IndexSearch(BlobRef_Idx, blobRef),
                new IndexSearch(Metadata_Name_Idx, "FileReference")).FirstOrDefault();

            if (fileReferenceMetadata is null)
                return null;

            var blobFileInfo = JsonConvert.DeserializeObject<BlobFileInfo>(fileReferenceMetadata.Value);

            return blobFileInfo;
        }

        public bool BlobExists(string blobRef)
        {
            return _azureBlobStore.FileExists(blobRef);
        }

        public (string BlobId, bool Created) PostBlob(Stream blob, string fileName, string hash)
        {
            var id = Guid.NewGuid().ToString();

            // TODO: check if we have previously stored this file via the hash


            // Add FileReference record containing the hash
            var mimeType = MimeTypeConverter.GetMimeType(fileName);
            var blobInfo = new BlobFileInfo(id, hash, fileName, mimeType, blob.Length, DateTime.UtcNow);

            var metaData = new ItemMetadata
            {
                BlobRef = id,
                Name = "FileReference",
                Value = JsonConvert.SerializeObject(blobInfo)
            };
            _aggregateState.LocalAdd(metaData);

            _azureBlobStore.UploadFile(id, blob);

            return (id, true);
        }

        public bool DeleteBlob(string blobRef)
        {
            try
            {
                DeleteMetadata(blobRef);
                
                _azureBlobStore.DeleteFile(blobRef);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
