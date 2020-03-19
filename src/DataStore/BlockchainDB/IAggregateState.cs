using System;
using System.Collections.Generic;
using DataStore.Blockchain;
using DataStore.P2P;

namespace DataStore.BlockchainDB
{
    public interface IAggregateState<T>
    {
        void WireUpNode(INode node);
        IndexDefinition<T> AddIndex(string name, Func<T, string> keyPointer);
        bool ProcessRemoteBlock(Block block); // Contains an instruction that could be Add or Remove from a remote node via P2P

        void LocalAdd(T item); // Appends an Add block locally and then broadcasts via P2P
        void LocalRemove(T item); // Appends a Remove block locally and then broadcasts via P2P

        void InitialiseChainFromBlobs();
        Block GetLatestBlock();

        Action<Block> OnLocalBlockAppended { set; }
        bool PersistChain { get; set; }
        
        IEnumerable<T> FindBySingleIndex(IndexSearch search);
        IEnumerable<T> FindByIntersection(IndexSearch search1, IndexSearch search2);
        IEnumerable<T> FindByIndexEvaluateValues(IndexSearch search, Func<T, bool> predicate);
        IEnumerable<T> AllItems();
    }
}