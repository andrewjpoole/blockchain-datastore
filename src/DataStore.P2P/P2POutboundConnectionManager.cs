using System;
using System.Collections.Generic;
using DataStore.Blockchain;
using DataStore.P2P.GrpcGen;
using Grpc.Core;
using Newtonsoft.Json;

namespace DataStore.P2P
{
    public class P2POutboundConnectionManager
    {
        public Dictionary<NodeAddress, OutboundConnection> OutboundConnections = new Dictionary<NodeAddress, OutboundConnection>();
        private NodeAddress localAddress;

        public void Connect(List<NodeAddress> allPortsInCluster, NodeAddress localAddress)
        {
            this.localAddress = localAddress;
            foreach (var nodeAddress in allPortsInCluster)
            {
                if (nodeAddress == localAddress)
                    continue;

                var channel = new Channel($"{nodeAddress.Host}:{nodeAddress.Port}", ChannelCredentials.Insecure);
                var client = new P2PGrpc.P2PGrpcClient(channel);

                OutboundConnections.Add(nodeAddress, new OutboundConnection { Channel = channel, Client = client });
            }
        }

        public void CloseAllConnections()
        {
            foreach (var outboundConnection in OutboundConnections.Values)
            {
                outboundConnection.Channel.ShutdownAsync().Wait();
            }
        }

        public void CloseConnection(NodeAddress node)
        {
            OutboundConnections[node].Channel.ShutdownAsync().Wait();
        }

        public void Broadcast(Request request, Action<Reply> onResponse)
        {
            foreach (var outboundConnection in OutboundConnections.Values)
            {
                var reply = outboundConnection.Client.Send(request);
                onResponse(reply);
            }
        }

        public void InitialiseNode(Action<Reply> onResponse)
        {
            foreach (var outboundConnection in OutboundConnections.Values)
            {
                var request = new Request { Mode = "InitNode", Data = "", From = $"node{this.localAddress}" };
                var reply = outboundConnection.Client.Send(request);
                onResponse(reply);
            }
        }

        public void ChallengeLatestBlockHashAcrossCluster(Block latestBlock, Action<Reply> onMisMatch)
        {
            foreach (var outboundConnection in OutboundConnections.Values)
            {
                var request = new Request { Mode = "Challenge", Data = JsonConvert.SerializeObject(latestBlock), From = $"node{this.localAddress}" };
                var reply = outboundConnection.Client.Send(request);

                if(reply.Data != "Ok")
                    onMisMatch(reply);
            }
        }

        // TODO Challenge method to confirm all the nodes are in agreement
        // TODO new node sync?


    }
}