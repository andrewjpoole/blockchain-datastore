using System;
using System.Collections.Generic;
using DataStore.Blockchain;
using DataStore.P2P.GrpcGen;
using Grpc.Core;
using Newtonsoft.Json;

namespace DataStore.P2P
{
    public class Node : INode, IDisposable
    {
        private readonly NodeAddress _localAddress;
        private Server _server;
        private P2PReceiverImpl _receiver;
        private P2POutboundConnectionManager _connectionManager;
        private Func<Block, bool> _onRemoteBlockArrived; // needs to be called when Send is received by the receiverImpl

        public Func<Block, bool> OnRemoteBlockArrived
        {
            set => _onRemoteBlockArrived = value;
        }

        public Node(
            NodeAddress localAddress,
            List<NodeAddress> allAddressesInCluster
            )
        {
            _localAddress = localAddress;

            _receiver = new P2PReceiverImpl((block) => _onRemoteBlockArrived.Invoke(block));
        
            _server = new Server
            {
                Services = { P2PGrpc.BindService(_receiver) },
                Ports = { new ServerPort(_localAddress.Host, _localAddress.Port, ServerCredentials.Insecure) }
            };
            _server.Start();

            _connectionManager = new P2POutboundConnectionManager();
            _connectionManager.Connect(allAddressesInCluster, _localAddress);
        }

        public async void Dispose()
        {
            _connectionManager.CloseAllConnections();
            await _server.ShutdownAsync();
        }
        
        public void BroadcastNewLocalBlock(Block newLocalBlock)
        {
            _connectionManager.Broadcast(new Request { From = $"node{_localAddress}", Mode = "Add", Data = JsonConvert.SerializeObject(newLocalBlock) },
                reply =>
                {
                    Console.WriteLine($"{reply.Data} from {reply.From}");

                    if (reply.Mode == "FailureToAddBlockResponse")
                    {
                        Console.WriteLine($"Removed last block!");
                    }
                });
        }
    }
}