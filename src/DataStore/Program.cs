using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DataStore.ConsulConfig;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DataStore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
            
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(cb =>
                {
                    var configuration = cb.Build();
                    cb.AddConsul(new[] { configuration.GetValue<Uri>("consulUri") }, configuration.GetValue<string>("consulKVPath"));
                })
                .UseStartup<Startup>();
    }
}
