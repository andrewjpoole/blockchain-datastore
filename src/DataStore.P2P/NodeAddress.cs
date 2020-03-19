namespace DataStore.P2P
{
    public class NodeAddress
    {
        public string Host { get; }
        public int Port { get; }

        public NodeAddress(string host, int port)
        {
            Host = host;
            Port = port;
        }

        public override string ToString()
        {
            return $"{Host}:{Port}";
        }
    }
}