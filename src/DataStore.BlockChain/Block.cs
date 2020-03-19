using Newtonsoft.Json;
using System;
using System.Security.Cryptography;
using System.Text;

namespace DataStore.Blockchain
{
    public class Block
    {
        public readonly int Index;
        public readonly DateTimeOffset TimeStamp;
        public readonly string PreviousHash;
        public readonly string Hash;
        public readonly Transaction Transaction;

        [JsonConstructor]
        public Block(DateTimeOffset timeStamp, int index, string hash, string previousHash, Transaction transaction)
        {
            Index = index;
            TimeStamp = timeStamp;
            PreviousHash = previousHash;
            Transaction = transaction;
            Hash = hash;
        }

        internal Block(DateTimeOffset timeStamp, int index, string previousHash, Transaction transaction)
        {
            Index = index;
            TimeStamp = timeStamp;
            PreviousHash = previousHash;
            Transaction = transaction;
            Hash = CalculateHash();
        }

        public string CalculateHash()
        {
            var sha256 = new SHA256Managed();
            var contentToHash = $"{TimeStamp}-{PreviousHash ?? ""}-{JsonConvert.SerializeObject(Transaction)}";
            var inputBytes = Encoding.ASCII.GetBytes(contentToHash);
            var outputBytes = sha256.ComputeHash(inputBytes);
            var hashString = Convert.ToBase64String(outputBytes);
            return hashString;
        }

        public override string ToString()
        {
            return $"{Transaction}";
        }
    }
}
