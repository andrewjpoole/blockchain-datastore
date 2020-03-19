using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DataStore.ConfigOptions;
using DataStore.Contracts;
using DataStore.Models;
using LiteDB;
using Microsoft.Extensions.Options;

namespace DataStore.LiteDB
{
    public class LiteDBRepository : IDataRepository
    {
        private readonly string dataLocation;

        public LiteDBRepository(
            IOptions<NodeAppSettingsOptions> appSettingsOptions
            )
        {
            var basePath = AppContext.BaseDirectory;
            dataLocation = Path.Combine(basePath, appSettingsOptions.Value.LiteDbDataLocation);
        }

        public IEnumerable<IItemMetadata> GetMetadata(string blobRef)
        {
            using (var db = new LiteDatabase(dataLocation))
            {
                var blobMetaDataCollection = db.GetCollection<ItemMetadata>("blobMetadata");

                var matches = blobMetaDataCollection.Find(x => x.BlobRef == blobRef);
                return matches.ToList();
            }
        }

        public IEnumerable<IItemMetadata> GetMetadata(string blobRef, string name)
        {
            using (var db = new LiteDatabase(dataLocation))
            {
                var blobMetaDataCollection = db.GetCollection<ItemMetadata>("blobMetadata");

                var matches = blobMetaDataCollection.Find(x => x.BlobRef == blobRef && x.Name == name);
                return matches.ToList();
            }
        }

        public void PostMetadata(string blobRef, string name, string value)
        {
            using (var db = new LiteDatabase(dataLocation))
            {
                InsertBlobMetadata(db, new ItemMetadata { Id = Guid.NewGuid().ToString(), BlobRef = blobRef, Name = name, Value = value });
            }
        }

        private string InsertBlobMetadata(LiteDatabase db, ItemMetadata newBlobMetadata)
        {
            var blobMetaDataCollection = db.GetCollection<ItemMetadata>("blobMetadata");
            blobMetaDataCollection.EnsureIndex(x => x.BlobRef, false);
            blobMetaDataCollection.EnsureIndex(x => x.Name);

            var result = blobMetaDataCollection.Insert(newBlobMetadata);
            return result;
        }

        public int DeleteMetadata(string blobRef, string name = "")
        {
            using (var db = new LiteDatabase(dataLocation))
            {
                var blobMetaDataCollection = db.GetCollection<ItemMetadata>("blobMetadata");
                int deletedCount;
                if (string.IsNullOrEmpty(name))
                {
                    deletedCount = blobMetaDataCollection.Delete(x => x.BlobRef == blobRef);
                }
                else
                {
                    deletedCount = blobMetaDataCollection.Delete(x => x.BlobRef == blobRef && x.Name == name);
                }
                return deletedCount;
            }
        }

        public int DeleteMetadata(string blobId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> FindBlobIdsViaMetadata(string name, string value, bool exactMatch)
        {
            using (var db = new LiteDatabase(dataLocation))
            {
                var blobMetaDataCollection = db.GetCollection<ItemMetadata>("blobMetadata");
                var matches = blobMetaDataCollection.Find(x => x.Name == name && x.Value.Contains(value, StringComparison.InvariantCultureIgnoreCase));
                return matches.ToList().Select(x => x.BlobRef);
            }
        }

        public int GetCountOfAllBlobReferences()
        {
            using (var db = new LiteDatabase(dataLocation))
            {
                var blobs = db.FileStorage.FindAll();
                return blobs.Count();
            }
        }

        public IEnumerable<IBlobFileInfo> GetAllBlobReferences(int itemsPerPage, int page)
        {
            using (var db = new LiteDatabase(dataLocation))
            {
                var blobs = db.FileStorage.FindAll();
                return blobs.ToList().Skip(itemsPerPage * page).Take(itemsPerPage).Select(fi => new BlobFileInfo(fi.Id, "hash", fi.Filename, fi.MimeType, fi.Length, fi.UploadDate));
            }
        }

        public IBlobFileInfo GetBlob(string blobRef, Stream output)
        {
            using (var db = new LiteDatabase(dataLocation))
            {
                if (!db.FileStorage.Exists(blobRef))
                {
                    return null;
                }

                var fileInfo = db.FileStorage.FindById(blobRef);

                if (fileInfo == null)
                    return null;

                var stream = fileInfo.OpenRead(); 

                stream.CopyTo(output);

                return new BlobFileInfo(fileInfo.Id, "hash", fileInfo.Filename, fileInfo.MimeType, fileInfo.Length, fileInfo.UploadDate);
            }
        }

        public bool BlobExists(string blobRef)
        {
            using (var db = new LiteDatabase(dataLocation))
            {
                return db.FileStorage.Exists(blobRef);
            }
        }

        public (string BlobId, bool Created) PostBlob(Stream blob, string fileName, string hash)
        {
            var id = Guid.NewGuid().ToString();

            using (var db = new LiteDatabase(dataLocation))
            {
                var blobMetaDataCollection = db.GetCollection<ItemMetadata>("blobMetadata");

                var existingBlob = blobMetaDataCollection.FindOne(x => x.Name == "SHA256" && x.Value == hash);
                if (existingBlob is null)
                {
                    // Upload file
                    blob.Position = 0;
                    var liteFileInfo = db.FileStorage.Upload(id, $"/blobs/{fileName}", blob);
                    
                    InsertBlobMetadata(db, new ItemMetadata { Id = Guid.NewGuid().ToString(), BlobRef = id, Name = "SHA256", Value = hash });

                    return (liteFileInfo.Id, true);
                }

                // return the ref of the existing blob
                return (existingBlob.BlobRef, false);
            }
        }

        public bool DeleteBlob(string blobRef)
        {
            DeleteMetadata(blobRef);

            using (var db = new LiteDatabase(dataLocation))
            {
                db.FileStorage.Delete(blobRef);
                return true;
            }
        }
    }
}
