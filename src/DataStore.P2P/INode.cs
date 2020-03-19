using System;
using DataStore.Blockchain;

namespace DataStore.P2P
{
    public interface INode
    {
        void BroadcastNewLocalBlock(Block newLocalBlock);
        Func<Block, bool> OnRemoteBlockArrived { set; }
    }
}