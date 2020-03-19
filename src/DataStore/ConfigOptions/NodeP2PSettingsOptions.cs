using System.Collections.Generic;

namespace DataStore.ConfigOptions
{
    public class NodeP2PSettingsOptions
    {
        public List<string> NodeAddresses { get; set; }
        public int Port { get; set; }
    }
}