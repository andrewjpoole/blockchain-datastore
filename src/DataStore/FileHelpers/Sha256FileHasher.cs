using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DataStore.FileHelpers
{
    public class Sha256FileHasher : IFileHasher
    {
        public string CalculateHash(string filePath)
        {
            var hasher = new SHA256Managed();
            var fileBytes = File.ReadAllBytes(filePath);
            var hashBytes = hasher.ComputeHash(fileBytes);
            var hash = BitConverter.ToString(hashBytes);
            return hash;
        }
    }
}