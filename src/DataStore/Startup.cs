using System;
using System.Diagnostics;
using System.Text;
using AsyncInternals;
using DataStore.Auth;
using DataStore.Blockchain;
using DataStore.BlockchainDB;
using DataStore.ConfigOptions;
using DataStore.FileHelpers;
using DataStore.LiteDB;
using DataStore.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using DataStore.Contracts;
using DataStore.P2P;
using Microsoft.AspNetCore.Http;
using Serilog.Core.Enrichers;
using Serilog.Sinks.Elasticsearch;
// ReSharper disable NotAccessedField.Local

namespace DataStore
{
    public class Startup
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = "yourdomain.com",
                        ValidAudience = "yourdomain.com",
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(Configuration["Node:AppSettings:SecurityKey"]))
                    };
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminsOnly", policy => policy.RequireClaim("Admin"));
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddOptions();
            services.Configure<NodeAppSettingsOptions>(Configuration.GetSection("Node:AppSettings"));
            services.Configure<NodeLoggerSettingsOptions>(Configuration.GetSection("Node:LoggerSettings"));
            services.Configure<NodeLoggerSettingsOptions>(Configuration.GetSection("Node:P2PSettings"));

            //services.AddHealthChecks().AddCheck("test", new TestHealthCheck());

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddSingleton<IUserManager>(new UserManager());
                                    
            services.AddSingleton<ITempFileHelper>(new TempFileHelper());

            services.AddSingleton<IFileTypeScanner>(new FileTypeScannerMyrmec());
            services.AddSingleton<IFileVirusScanner>(new FileVirusScannerWindowsDefender());
            services.AddSingleton<IFileHasher>(new Sha256FileHasher());
            
            var logger = new LoggerConfiguration()
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(Configuration["Node:LoggerSettings:elasticUri"]))
                {
                    IndexFormat = Configuration["Node:LoggerSettings:indexPattern"],
                    AutoRegisterTemplate = true,
                    AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv6
                })
                .Enrich.FromLogContext()
                .Enrich.With(new PropertyEnricher("Node", $"{Environment.MachineName}:{Process.GetCurrentProcess().Id}"))
                .Enrich.With(new PropertyEnricher("Env", $"{Configuration["Node:LoggerSettings:env"]}"))
                .CreateLogger();
            services.AddSingleton<ILogger>(logger);

            //services.AddSingleton<IDataRepository, LiteDBRepository>();

            services.AddSingleton<IDateTimeOffsetProvider, DateTimeOffsetProvider > ();
            services.AddSingleton<IChain, Chain>();
            services.AddSingleton<INode, Node>();
            services.AddSingleton<IAggregateState<ItemMetadata>, AggregatedState<ItemMetadata>>();
            services.AddSingleton<IAzureBlobStore, AzureBlobStore>();
            services.AddSingleton<IDataRepository, BlockchainDBRepository>();

            services.AddSwaggerDocumentation();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseAuthentication();
            //app.UseHealthChecks("/healthcheck");
            
            app.UseSwaggerDocumentation();
            
            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }

    //public class TestHealthCheck : IHealthCheck
    //{
    //    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    //    {
    //        return Task.Factory.StartNew(() => 
    //        {
    //            return new HealthCheckResult(true, "a test description", null, new Dictionary<string, object>());
    //        });
    //    }
    //}
}
