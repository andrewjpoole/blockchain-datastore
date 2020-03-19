﻿using DataStore.P2P.TestAppN1;
using System;

namespace DataStore.P2P.TestAppN2
{
    class Program
    {
        static void Main(string[] args)
        {
            var serverPort = int.Parse(args[0]);

            var testApp = new TestApp(new NodeAddress("127.0.0.1", serverPort));
            testApp.Run();

        }
    }
}
