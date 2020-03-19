using DataStore.P2P.GrpcGen;
using Grpc.Core;

namespace DataStore.P2P
{
    public class OutboundConnection
    {
        public P2PGrpc.P2PGrpcClient Client;
        public Channel Channel;
    }
}