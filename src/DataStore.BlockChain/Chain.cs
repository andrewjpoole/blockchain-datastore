using AsyncInternals;
using System.Collections.Generic;
using System.Linq;
using System;
using Newtonsoft.Json;

namespace DataStore.Blockchain
{
    public class Chain : IChain
    {
        [JsonIgnore]
        public IDateTimeOffsetProvider DateTimeOffsetProvider;

        public IList<Block> Blocks { set; get; }

        [JsonConstructor]
        public Chain(IList<Block> Blocks)
        {
            this.Blocks = Blocks;
        }

        public Chain(IDateTimeOffsetProvider dateTimeOffsetProvider)
        {
            DateTimeOffsetProvider = dateTimeOffsetProvider;
            Blocks = new List<Block>();
            var genesisBlock = new Block(new DateTimeOffset(), 0, "", null);
            Blocks.Add(genesisBlock);
        }

        public int Count => Blocks.Count;
        public Action<Block> OnBlockAdded { get; set; }

        public Block GetLatestBlock()
        {
            return Blocks[Blocks.Count - 1];
        }

        public Block GetPreviousBlock()
        {
            if (Blocks.Count == 1)
                return Blocks[0];
            else
                return Blocks[Blocks.Count - 1];
        }

        public object GetChain()
        {
            return Blocks.Select(b => new { b.Index, b.TimeStamp, b.Transaction, b.Hash, b.PreviousHash});
        }

        public Block CreateBlock(Transaction transaction)
        {
            var latestBlock = GetLatestBlock();
            var newblock = new Block(DateTimeOffsetProvider.GetDateTimeOffset(), latestBlock.Index + 1, latestBlock.Hash, transaction);
            return newblock;
        }

        public void AddBlock(Block newblock)
        {
            Blocks.Add(newblock);
            OnBlockAdded?.Invoke(newblock);
        }

        public void UndoLastBlock()
        {
            // Only if didn't receive Ok from other nodes etc...
            Blocks.RemoveAt(Blocks.Count - 1);
        }

        public bool TryValidateRemoteBlock(Block remoteNewBlock)
        {
            // validate the last 100 blocks in the chain...
            if (!Validate(100))
            {
                // the chain is corrupted, TODO request a copy of someone elses?
                // or remove blocks from the end until it is valid again
                return false;
            }

            if (remoteNewBlock.Hash != remoteNewBlock.CalculateHash())
            {
                return false;
            }

            var previousBlock = GetPreviousBlock();

            if (remoteNewBlock.PreviousHash != previousBlock.Hash)
            {
                return false;
            }

            return true;

        }

        public bool Validate(int numberOfBlocksToVerify = 1)
        {
            int startIndex;
            if (numberOfBlocksToVerify == 1 || numberOfBlocksToVerify >= Blocks.Count)
            {
                startIndex = 1;
            }
            else
            {
                startIndex = Blocks.Count - numberOfBlocksToVerify;
            }
            
            for (int i = startIndex; i < Blocks.Count; i++)
            {
                var currentBlock = Blocks[i];
                var previousBlock = Blocks[i - 1];

                if (currentBlock.Hash != currentBlock.CalculateHash())
                {
                    return false;
                }

                if (currentBlock.PreviousHash != previousBlock.Hash)
                {
                    return false;
                }
            }
            return true;
        }

        public override string ToString()
        {
            return $"Chain Count:{Blocks.Count} Latest:{GetLatestBlock()}";
        }
    }
}
