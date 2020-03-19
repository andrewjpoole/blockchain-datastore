using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DataStore.Blockchain;
using DataStore.P2P;
using Newtonsoft.Json;

namespace DataStore.BlockchainDB
{
    public class AggregatedState<T> : IAggregateState<T>
    {
        private readonly IAzureBlobStore _azureBlobStore;
        private readonly IChain _chain;
        private readonly IndexedList<T> _state = new IndexedList<T>();
        private Action<Block> _onLocalBlockAppended; // Invoke when a local block has been appended
        public Action<Block> OnLocalBlockAppended { set => _onLocalBlockAppended = value; }
        public bool PersistChain { get; set; }
        
        public AggregatedState(
            IChain chain,
            IAzureBlobStore azureBlobStore
        )
        {
            _azureBlobStore = azureBlobStore;
            _azureBlobStore.Connect();
            _chain = chain;
            _chain.OnBlockAdded = UpdateIndexedlistFromNewBlock;
        }

        public IndexDefinition<T> AddIndex(string name, Func<T, string> keyPointer)
        {
            return _state.AddIndex(name, keyPointer);
        }

        public Block GetLatestBlock()
        {
            return _chain.GetLatestBlock();
        }
        
        public IEnumerable<T> FindBySingleIndex(IndexSearch search)
        {
            return _state.FindBySingleIndex(search);
        }

        public IEnumerable<T> FindByIntersection(IndexSearch search1, IndexSearch search2)
        {
            return _state.FindByIntersection(search1, search2);
        }

        public IEnumerable<T> FindByIndexEvaluateValues(IndexSearch search, Func<T, bool> predicate)
        {
            return _state.FindByIndexEvaluateValues(search, predicate);
        }

        public void WireUpNode(INode node)
        {
            _onLocalBlockAppended = (newLocalBlock) => node.BroadcastNewLocalBlock(newLocalBlock);
            node.OnRemoteBlockArrived = (newRemoteBlock) => ProcessRemoteBlock(newRemoteBlock);
        }

        private void UpdateIndexedlistFromNewBlock(Block block)
        {
            // update indexed list
            var instruction = block.Transaction.GetMetadataInstruction();

            var item = instruction.GetData<T>();
            switch (instruction.Action)
            {
                case Actions.Add:
                    _state.Add(item);
                    break;
                case Actions.Remove:
                    _state.Remove(item);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
        
        public IEnumerable<T> AllItems()
        {
            return _state.ToList();
        }

        // Add Block from remote P2P node to local chain
        public bool ProcessRemoteBlock(Block newRemoteBlock)
        {
            if (_chain.TryValidateRemoteBlock(newRemoteBlock))
            {
                _chain.AddBlock(newRemoteBlock);
                return true;
            }

            return false;
        }

        // Append an Add block to the local chain and then broadcast
        public void LocalAdd(T item)
        {
            var serialisedMetadata = JsonConvert.SerializeObject(item);
            var metadataInstruction = new MetadataInstruction { Action = Actions.Add, Data = serialisedMetadata};
            var trans = new Transaction("", "Add", metadataInstruction);
            var block = _chain.CreateBlock(trans);
            _chain.AddBlock(block);

            _onLocalBlockAppended?.Invoke(_chain.GetLatestBlock());

            PersistChainToBlob();
        }

        // Append a Remove clock to the local chain and then broadcast
        public void LocalRemove(T item)
        {
            var serialisedMetadata = JsonConvert.SerializeObject(item);
            var metadataInstruction = new MetadataInstruction { Action = Actions.Remove, Data = serialisedMetadata };
            var trans = new Transaction("", "Add", metadataInstruction);
            var block = _chain.CreateBlock(trans);
            _chain.AddBlock(block);

            _onLocalBlockAppended?.Invoke(_chain.GetLatestBlock());

            PersistChainToBlob();
        }

        private void PersistChainToBlob()
        {
            if (!PersistChain)
                return;

            var serialisedChain = JsonConvert.SerializeObject(_chain.Blocks);
            using (var ms = new MemoryStream())
            {
                TextWriter tw = new StreamWriter(ms);
                tw.Write(serialisedChain);
                tw.Flush();
                ms.Position = 0;

                _azureBlobStore.UploadFile("chain", ms);
            }
        }

        public void InitialiseChainFromBlobs()
        {
            var serialisedBlocks = "";
            using (var ms = _azureBlobStore.DownloadFile("chain"))
            {
                var reader = new StreamReader(ms);
                serialisedBlocks = reader.ReadToEnd();
            }
            var blocks = JsonConvert.DeserializeObject<List<Block>>(serialisedBlocks);
            foreach (var block in blocks.Skip(1)) // genesis block will already be present
            {
                _chain.AddBlock(block); // this will also update the IndexedList state
            }
        }
    }
}