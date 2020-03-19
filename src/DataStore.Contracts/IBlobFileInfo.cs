using System;
using System.Collections.Generic;

namespace DataStore.LiteDB
{
    public interface IBlobFileInfo
    {
        string Id { get; }
        string Filename { get; }
        string MimeType { get; }
        long Length { get; }
        DateTime UploadDate { get; }
    }
}
