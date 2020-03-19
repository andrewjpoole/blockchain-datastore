using System;
using DataStore.LiteDB;

namespace DataStore.Models
{
    public class BlobFileInfo : IBlobFileInfo
    {
        public string Id { get; private set; }
        public string Hash { get; private set; }
        public string Filename { get; private set; }
        public string MimeType { get; private set; }
        public long Length { get; private set; }
        public DateTime UploadDate { get; private set; }

        public BlobFileInfo(string id, string hash, string filename, string mimeType, long length, DateTime uploadDate)
        {
            Id = id;
            Hash = hash;
            Filename = filename;
            MimeType = mimeType;
            Length = length;
            UploadDate = uploadDate;
        }
    }
}
