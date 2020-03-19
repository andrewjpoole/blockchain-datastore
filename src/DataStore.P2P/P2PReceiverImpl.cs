using System;
using System.Threading.Tasks;
using DataStore.Blockchain;
using DataStore.P2P.GrpcGen;
using Grpc.Core;
using Newtonsoft.Json;

namespace DataStore.P2P
{
    public class P2PReceiverImpl : P2PGrpc.P2PGrpcBase
    {
        private readonly Func<Block, bool> _onRemoteBlockReceived;
        private JsonSerializerSettings serialisationSettings = new JsonSerializerSettings{ TypeNameHandling = TypeNameHandling.All };

        public P2PReceiverImpl(Func<Block, bool> onRemoteBlockReceived)
        {
            _onRemoteBlockReceived = onRemoteBlockReceived;
        }

        // Server side handler of the Send RPC i.e. we are receiving the sent messages from another node
        public override Task<Reply> Send(Request request, ServerCallContext context)
        {
            return Task.Factory.StartNew(() => 
            {
                Console.WriteLine($"received {request.Mode} Request from {request.From} on {context.Peer} data:{request.Data}");

                switch (request.Mode)
                {
                    case "Hello":
                        return new Reply { Mode = "HelloResponse", Data = $"Hello {request.Data}@{request.From}", From = $"{context.Host}" };
                        
                    case "Add":                        
                        var newRemoteBlock = JsonConvert.DeserializeObject<Block>(request.Data, serialisationSettings);
                        if(_onRemoteBlockReceived.Invoke(newRemoteBlock))
                            return new Reply { Mode = "AddResponse", Data = $"Added Block", From = $"{context.Host}" };
                        else
                            return new Reply { Mode = "FailureToAddBlockResponse", Data = $"Did not add Block", From = $"{context.Host}" };

                        //if (_getChain().TryValidateRemoteBlock(newRemoteBlock))
                        //{
                        //    _getChain().AddBlock(newRemoteBlock);
                        //    Console.WriteLine("Added block locally");
                        //    return new Reply { Mode = "AddResponse", Data = $"Added Block", From = $"{context.Host}" };
                        //}
                        //else
                        //{
                        //    return new Reply { Mode = "FailureToAddBlockResponse", Data = $"Did not add Block", From = $"{context.Host}" };
                        //}
                    //case "InitNode":
                    //    // if we are a replica/quorum then send the chain 100 blocks at a time?
                    //    return new Reply { Mode = "Chain", Data = JsonConvert.SerializeObject(_getChain()), From = $"{context.Host}" };

                    //case "Challenge":
                    //    // A node wants to check that its last block matches all other nodes
                    //    var remoteBlockToCheck = JsonConvert.DeserializeObject<Block>(request.Data, serialisationSettings);
                    //    var localLastBlock = _getChain().GetLatestBlock();
                    //    var response = localLastBlock.Hash == remoteBlockToCheck.Hash ? "Ok" : localLastBlock.Hash;
                    //    return new Reply { Mode = "ChallengeResponse", Data = response, From = $"{context.Host}" };

                    default:
                        return new Reply { Mode = "Response", Data = $"Didn't recognise the mode {request.Mode}", From = $"{context.Host}" };
                        
                }
            });
        }
    }
}
