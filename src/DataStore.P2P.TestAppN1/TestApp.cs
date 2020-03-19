using AsyncInternals;
using DataStore.Blockchain;
using Grpc.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using DataStore.P2P.GrpcGen;

namespace DataStore.P2P.TestAppN1
{
    public class TestApp
    {
        private readonly NodeAddress _localAddress;

        public TestApp(NodeAddress localAddress)
        {
            _localAddress = localAddress;
        }

        public void Run()
        {
            var dateTimeProvider = new DateTimeOffsetProvider();
            var chain = new Chain(dateTimeProvider);

            Server server = new Server
            {
                Services = { P2PGrpc.BindService(new P2PReceiverImpl(OnRemoteBlockReceived)) },
                Ports = { new ServerPort(_localAddress.Host, _localAddress.Port, ServerCredentials.Insecure) }
            };
            server.Start();
            Console.WriteLine("P2PGrpc server listening on port " + _localAddress.Port);
            
            // client connection to servers
            var manager = new P2POutboundConnectionManager();
            var allPortsInCluster = new List<NodeAddress> { new NodeAddress("localhost", 6000), new NodeAddress("localhost", 6001), new NodeAddress("localhost", 6002) };
            manager.Connect(allPortsInCluster, _localAddress);
            manager.InitialiseNode(reply => 
            {
                var seedChain = JsonConvert.DeserializeObject<Chain>(reply.Data);
                seedChain.DateTimeOffsetProvider = dateTimeProvider;
                chain = seedChain;
            });

            DisplayHelp();
            
            while (true)
            {
                var command = Console.ReadLine();
                switch (command)
                {
                    case "send":
                        Console.WriteLine("enter a name...");
                        var user = Console.ReadLine();
                        manager.Broadcast(new Request { From = $"node{_localAddress}", Mode = "Hello", Data = user }, reply => { Console.WriteLine($"{reply.Data} from {reply.From}"); });
                                                    
                        break;
                    case "add":
                        Console.WriteLine("enter data:");
                        var data = Console.ReadLine();

                        var newTransaction = new Transaction($"node{_localAddress}", "Add", data);
                        var newBlock = chain.CreateBlock(newTransaction);
                        chain.AddBlock(newBlock);

                        manager.Broadcast(new Request { From = $"node{_localAddress}", Mode = "Add", Data = JsonConvert.SerializeObject(chain.GetLatestBlock()) }, 
                            reply => 
                            {
                                Console.WriteLine($"{reply.Data} from {reply.From}");

                                if (reply.Mode == "FailureToAddBlockResponse")
                                {
                                    chain.UndoLastBlock();
                                    Console.WriteLine($"Removed last block!");
                                }
                            });
                        
                        // TODO manage failed message coming back...

                        break;
                    case "addlocal":
                        Console.WriteLine("enter data:");
                        var dataLocal = Console.ReadLine();

                        var newTransactionLocal = new Transaction($"node{_localAddress}", "Add", dataLocal);
                        var newBlockLocal = chain.CreateBlock(newTransactionLocal);
                        chain.AddBlock(newBlockLocal);
                        // This would simulate what would happen if this node was truncated?                        

                        break;
                    case "challenge":
                        manager.ChallengeLatestBlockHashAcrossCluster(chain.GetLatestBlock(), reply =>
                        {
                            Console.WriteLine($"node {reply.From} has mismatched latest hash of {reply.Data}");
                        });
                        break;
                    case "display":
                        Console.WriteLine(JsonConvert.SerializeObject(chain.GetChain(), Formatting.Indented));
                        break;
                    case "help":
                        DisplayHelp();
                        break;

                    case "exit":
                        server.ShutdownAsync().Wait();
                        manager.CloseAllConnections();
                        Environment.Exit(0);
                        break;
                }
            }
        }

        private bool OnRemoteBlockReceived(Block arg)
        {
            throw new NotImplementedException();
        }

        private void DisplayHelp()
        {
            Console.WriteLine("options:");
            Console.WriteLine("send [to send message to server]");
            Console.WriteLine("add [to add a block to the chain and broadcast it]");
            Console.WriteLine("addlocal [to add a block to the chain without broadcasting it]");
            Console.WriteLine("challenge [to check if the latest block hash matches across the cluster]");
            Console.WriteLine("display [to display the blockchain]");
            Console.WriteLine("help [for these instructions again] or exit...");
        }
    }
}
