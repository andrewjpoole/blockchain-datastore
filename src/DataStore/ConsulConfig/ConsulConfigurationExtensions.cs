using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace DataStore.ConsulConfig
{
    /// <summary>
    /// Taken from excellent article at https://www.natmarchand.fr/consul-configuration-aspnet-core/
    /// </summary>
    public static class ConsulConfigurationExtensions
    {
        public static IConfigurationBuilder AddConsul(this IConfigurationBuilder configurationBuilder, IEnumerable<Uri> consulUrls, string consulPath)
        {
            return configurationBuilder.Add(new ConsulConfigurationSource(consulUrls, consulPath));
        }

        public static IConfigurationBuilder AddConsul(this IConfigurationBuilder configurationBuilder, IEnumerable<string> consulUrls, string consulPath)
        {
            return configurationBuilder.AddConsul(consulUrls.Select(u => new Uri(u)), consulPath);
        }
    }
}