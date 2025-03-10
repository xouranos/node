﻿using System;
using System.Text;
using System.Threading.Tasks;
using Martiscoin.Builder;
using Martiscoin.Builder.Feature;
using Martiscoin.Configuration;
using Martiscoin.Configuration.Logging;
using Martiscoin.Features.Consensus;
using Martiscoin.Networks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Martiscoin.Features.RPC
{
    public interface IRPCFeature
    {
        IWebHost RPCHost { get; set; }
    }

    public class RPCFeature : FullNodeFeature, IRPCFeature
    {
        /// <summary>How long we are willing to wait for the NodeHost to stop.</summary>
        private const int NodeHostStopTimeoutSeconds = 10;

        private readonly FullNode fullNode;

        private readonly NodeSettings nodeSettings;

        private readonly ILogger logger;

        private readonly IFullNodeBuilder fullNodeBuilder;

        private readonly RpcSettings rpcSettings;

        public IWebHost RPCHost { get; set; }

        public RPCFeature(IFullNodeBuilder fullNodeBuilder, FullNode fullNode, NodeSettings nodeSettings, ILoggerFactory loggerFactory, RpcSettings rpcSettings)
        {
            this.fullNodeBuilder = fullNodeBuilder;
            this.fullNode = fullNode;
            this.nodeSettings = nodeSettings;
            this.logger = loggerFactory.CreateLogger(this.GetType().FullName);
            this.rpcSettings = rpcSettings;
        }

        /// <summary>
        /// Prints command-line help.
        /// </summary>
        /// <param name="network">The network to extract values from.</param>
        public static void PrintHelp(Network network)
        {
            RpcSettings.PrintHelp(network);
        }

        /// <summary>
        /// Get the default configuration.
        /// </summary>
        /// <param name="builder">The string builder to add the settings to.</param>
        /// <param name="network">The network to base the defaults off.</param>
        public static void BuildDefaultConfigurationFile(StringBuilder builder, Network network)
        {
            //builder.AppendLine("####RPC Settings####");
            //builder.AppendLine("#Activate RPC Server (default: 0)");
            //builder.AppendLine("#server=0");
            //builder.AppendLine("#Where the RPC Server binds (default: 127.0.0.1 and ::1)");
            //builder.AppendLine("#rpcbind=127.0.0.1");
            //builder.AppendLine("#Ip address allowed to connect to RPC (default all: 0.0.0.0 and ::)");
            //builder.AppendLine("#rpcallowip=127.0.0.1");
            //builder.AppendLine("#Can load the RPCContentType with or without charset. (default: application/json; chartset=utf-8)");
            //builder.AppendLine("#rpccontenttype=application/json");

        }

        public override Task InitializeAsync()
        {
            if (this.rpcSettings.Server)
            {
                // TODO: The web host wants to create IServiceProvider, so build (but not start)
                // earlier, if you want to use dependency injection elsewhere
                
                var webHost = new WebHostBuilder()
                .UseKestrel(o => o.AllowSynchronousIO = true)
                .ForFullNode(this.fullNode)
                .UseUrls(this.rpcSettings.GetUrls())
                .UseIISIntegration()
                .ConfigureServices(collection =>
                {
                    if (this.fullNodeBuilder != null && this.fullNodeBuilder.Services != null && this.fullNode != null)
                    {
                        // copies all the services defined for the full node to the Api.
                        // also copies over singleton instances already defined
                        foreach (ServiceDescriptor service in this.fullNodeBuilder.Services)
                        {
                            // open types can't be singletons
                            if (service.ServiceType.IsGenericType || service.Lifetime == ServiceLifetime.Scoped)
                            {
                                collection.Add(service);
                                continue;
                            }

                            object obj = this.fullNode.Services.ServiceProvider.GetService(service.ServiceType);

                            if (obj != null && service.Lifetime == ServiceLifetime.Singleton && service.ImplementationInstance == null)
                            {
                                collection.AddSingleton(service.ServiceType, obj);
                            }
                            else
                            {
                                collection.Add(service);
                            }
                        }
                    }
                })
                .UseStartup<Startup>()
                .Build();

                webHost.Start();
                this.RPCHost = webHost;
                this.logger.LogInformation("RPC listening on: " + Environment.NewLine + string.Join(Environment.NewLine, this.rpcSettings.GetUrls()));
            }
            else
            {
                this.logger.LogInformation("RPC Server is off based on configuration.");
            }
            
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            // Make sure we are releasing the listening ip address / port.
            if (this.RPCHost != null)
            {
                this.logger.LogInformation("Disposing RPC host.");
                this.RPCHost.StopAsync(TimeSpan.FromSeconds(NodeHostStopTimeoutSeconds)).Wait();
                this.RPCHost = null;
            }
        }
    }

    /// <summary>
    /// A class providing extension methods for <see cref="IFullNodeBuilder"/>.
    /// </summary>
    public static class FullNodeBuilderRPCExtension
    {
        public static IFullNodeBuilder AddRPC(this IFullNodeBuilder fullNodeBuilder)
        {
            LoggingConfiguration.RegisterFeatureNamespace<RPCFeature>("rpc");

            fullNodeBuilder.ConfigureFeature(features =>
            {
                features
                .AddFeature<RPCFeature>()
                .DependOn<ConsensusFeature>()
                .FeatureServices(services =>
                {
                    services.AddSingleton(fullNodeBuilder);
                    services.AddSingleton<IRPCFeature>(provider => provider.GetService<RPCFeature>());
                });
            });

            fullNodeBuilder.ConfigureServices(service =>
            {
                service.AddSingleton<RpcSettings>();
                service.AddSingleton<IRPCClientFactory, RPCClientFactory>();
            });

            return fullNodeBuilder;
        }
    }
}
