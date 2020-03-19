using System.Collections.Generic;
using System.IO;
using System.Linq;
using Myrmec;
#pragma warning disable 618

namespace DataStore.FileHelpers
{
    public class FileTypeScannerMyrmec : IFileTypeScanner
    {
        public string ExtensionFromFile(string path)
        {
            Sniffer sniffer = new Sniffer();

            sniffer.Populate(FileTypes.Common);

            // get file head byte, first 20 bytes should be enough.
            byte[] fileHead = ReadFileHead(path);
            
            List<string> results = sniffer.Match(fileHead, false);
            return results.First();
        }

        public (bool FileTypeIsOk, string FileType) CheckFileType(string path)
        {
            var allowedFileTypes = new List<string> {"jpg", "pdf", "bmp", "tif"};

            var fileType = ExtensionFromFile(path);

            return allowedFileTypes.Contains(fileType) ? (true, fileType) : (false, fileType);
        }

        private byte[] ReadFileHead(string path)
        {
            using (var fileStream = File.OpenRead(path))
            {
                var headerBytes = new byte[20];
                for (int i = 0; i < 20; i++)
                {
                    headerBytes[i] = (byte) fileStream.ReadByte();
                }
                return headerBytes;
            }
        }
    }
}