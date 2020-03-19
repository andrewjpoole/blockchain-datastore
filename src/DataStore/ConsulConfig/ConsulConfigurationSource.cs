using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace DataStore.ConsulConfig
{
    /// <summary>
    /// Taken from excellent article at https://www.natmarchand.fr/consul-configuration-aspnet-core/
    /// </summary>
    public class ConsulConfigurationSource : IConfigurationSource
    {
        public IEnumerable<Uri> ConsulUrls { get; }
        public string Path { get; }

        public ConsulConfigurationSource(IEnumerable<Uri> consulUrls, string path)
        {
            ConsulUrls = consulUrls;
            Path = path;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new ConsulConfigurationProvider(ConsulUrls, Path);
        }
    }
}