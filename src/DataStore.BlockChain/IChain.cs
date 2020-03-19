using System;
using System.Collections.Generic;

namespace DataStore.Blockchain
{
    public interface IChain
    {
        IList<Block> Blocks { set; get; }
        int Count { get; }
        Action<Block> OnBlockAdded { get; set; }
        Block GetLatestBlock();
        Block GetPreviousBlock();
        object GetChain();
        Block CreateBlock(Transaction transaction);
        void AddBlock(Block newblock);
        void UndoLastBlock();
        bool TryValidateRemoteBlock(Block remoteNewBlock);
        bool Validate(int numberOfBlocksToVerify = 1);
    }
}