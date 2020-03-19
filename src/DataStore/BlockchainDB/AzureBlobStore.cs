using System;
using System.IO;
using DataStore.ConfigOptions;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Options;
using Serilog;

namespace DataStore.BlockchainDB
{
    public class AzureBlobStore : IAzureBlobStore
    {
        private readonly ILogger _logger;
        private readonly NodeAppSettingsOptions _nodeAppSettingsOptions;
        private CloudStorageAccount _cloudStorageAccount;
        private CloudBlobClient _blobClient;
        private CloudBlobContainer _blobContainer;

        public AzureBlobStore(
            ILogger logger,
            IOptions<NodeAppSettingsOptions> nodeAppSettingsOptions
        )
        {
            _logger = logger;
            _nodeAppSettingsOptions = nodeAppSettingsOptions.Value;
        }
        
        public void Connect()
        {
            if (_cloudStorageAccount != null)
                return;

            if (CloudStorageAccount.TryParse(_nodeAppSettingsOptions.AzureBlobStoreConnectionString, out _cloudStorageAccount))
            {
                _logger.Verbose("Logged into Azure Cloud Storage account");

                _blobClient = _cloudStorageAccount.CreateCloudBlobClient();
                _blobContainer = _blobClient.GetContainerReference(_nodeAppSettingsOptions.AzureBlobContainerReference);
                _blobContainer.CreateIfNotExists();
            }
            else
            {
                _logger.Error("Unable to log into Azure Cloud Storage account");
                throw new ApplicationException("Unable to log into Azure Cloud Storage account");
            }
        }

        public void UploadFile(string blobRef, Stream fileStream)
        {
            _logger.Verbose("Uploading file to Azure Cloud Storage account");
            var blob = _blobContainer.GetBlockBlobReference(blobRef);
            blob.UploadFromStream(fileStream);
            _logger.Verbose("Uploaded file to Azure Cloud Storage account");
        }

        public Stream DownloadFile(string blobRef)
        {
            _logger.Verbose("Downloading file from Azure Cloud Storage account");
            var blob = _blobContainer.GetBlockBlobReference(blobRef);
            var stream = new MemoryStream();
            blob.DownloadToStream(stream);
            stream.Position = 0;
            return stream;
        }

        public void DeleteFile(string blobRef)
        {
            _logger.Verbose("Deleting file from Azure Cloud Storage account");
            var blob = _blobContainer.GetBlockBlobReference(blobRef);
            blob.DeleteIfExists(DeleteSnapshotsOption.IncludeSnapshots);
        }

        public bool FileExists(string blobRef)
        {
            _logger.Verbose("Checking file exists from Azure Cloud Storage account");
            var blob = _blobContainer.GetBlockBlobReference(blobRef);
            return blob.Exists();
        }
    }
}