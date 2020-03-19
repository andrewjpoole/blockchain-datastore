using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AsyncInternals;
using DataStore.Blockchain;
using DataStore.BlockchainDB;
using DataStore.P2P;
using NUnit.Framework;

namespace DataStore.Tests
{
    [TestFixture]
    public class P2PIntegrationTests
    {
        private IDateTimeOffsetProvider _dateTimeOffsetProvider;
        private IChain _n1Chain;
        private IChain _n2Chain;
        private IChain _n3Chain;
        private NodeAddress _n1Address;
        private NodeAddress _n2Address;
        private NodeAddress _n3Address;
        private List<NodeAddress> _addresses;
        private Node _node1;
        private Node _node2;
        private Node _node3;
        private AggregatedState<string> _state1;
        private AggregatedState<string> _state2;
        private AggregatedState<string> _state3;

        [SetUp]
        public void SetUp()
        {
            _dateTimeOffsetProvider = new DateTimeOffsetProvider();

            _n1Chain = new Chain(_dateTimeOffsetProvider);
            _n2Chain = new Chain(_dateTimeOffsetProvider);
            _n3Chain = new Chain(_dateTimeOffsetProvider);

            _state1 = new AggregatedState<string>(_n1Chain);
            _state2 = new AggregatedState<string>(_n2Chain);
            _state3 = new AggregatedState<string>(_n3Chain);
            
        }

        [TearDown]
        public void TearDown()
        {
            _node1?.Dispose();
            _node2?.Dispose();
            _node3?.Dispose();
        }

        [Test]
        public async Task TestThatTwoNodesCanSynchronise()
        {
            _n1Address = new NodeAddress("127.0.0.1", 6101);
            _n2Address = new NodeAddress("127.0.0.1", 6102);

            _addresses = new List<NodeAddress> { _n1Address, _n2Address };

            _node1 = new Node(_n1Address, _addresses);
            _node2 = new Node(_n2Address, _addresses);

            _state1.WireUpNode(_node1);
            _state2.WireUpNode(_node2);

            _state1.LocalAdd("someData");
            
            Assert.That(_state1.GetLatestBlock().Transaction.GetData<string>(), Is.EqualTo("someData"));
            Assert.That(_state2.GetLatestBlock().Transaction.GetData<string>(), Is.EqualTo("someData"));
        }

        [Test]
        public async Task TestThatThreeNodesCanSynchronise()
        {
            _n1Address = new NodeAddress("127.0.0.1", 6111);
            _n2Address = new NodeAddress("127.0.0.1", 6112);
            _n3Address = new NodeAddress("127.0.0.1", 6113);

            _addresses = new List<NodeAddress> { _n1Address, _n2Address, _n3Address };

            _node1 = new Node(_n1Address, _addresses);
            _node2 = new Node(_n2Address, _addresses);
            _node3 = new Node(_n3Address, _addresses);

            _state1.WireUpNode(_node1);
            _state2.WireUpNode(_node2);
            _state3.WireUpNode(_node3);

            _state1.LocalAdd("someData");

            Assert.That(_state1.GetLatestBlock().Transaction.GetData<string>(), Is.EqualTo("someData"));
            Assert.That(_state2.GetLatestBlock().Transaction.GetData<string>(), Is.EqualTo("someData"));
            Assert.That(_state3.GetLatestBlock().Transaction.GetData<string>(), Is.EqualTo("someData"));
        }

        [Test]
        public async Task TestThatThreeNodesRemainSynchronisedAfter10AddsToNode1()
        {
            _n1Address = new NodeAddress("127.0.0.1", 6121);
            _n2Address = new NodeAddress("127.0.0.1", 6122);
            _n3Address = new NodeAddress("127.0.0.1", 6123);

            _addresses = new List<NodeAddress> { _n1Address, _n2Address, _n3Address };

            _node1 = new Node(_n1Address, _addresses);
            _node2 = new Node(_n2Address, _addresses);
            _node3 = new Node(_n3Address, _addresses);

            _state1.WireUpNode(_node1);
            _state2.WireUpNode(_node2);
            _state3.WireUpNode(_node3);
            
            _state1.LocalAdd("someData1");
            _state1.LocalAdd("someData2");
            _state1.LocalAdd("someData3");
            _state1.LocalAdd("someData4");
            _state1.LocalAdd("someData5");
            _state1.LocalAdd("someData6");
            _state1.LocalAdd("someData7");
            _state1.LocalAdd("someData8");
            _state1.LocalAdd("someData9");
            _state1.LocalAdd("someData10");
            
            Assert.That(_state1.GetLatestBlock().Transaction.GetData<string>(), Is.EqualTo("someData10"));
            Assert.That(_state2.GetLatestBlock().Transaction.GetData<string>(), Is.EqualTo("someData10"));
            Assert.That(_state3.GetLatestBlock().Transaction.GetData<string>(), Is.EqualTo("someData10"));
        }

        [Test]
        public async Task TestThatThreeNodesRemainSynchronisedAfter10AddsToAllNodes()
        {
            _n1Address = new NodeAddress("127.0.0.1", 6131);
            _n2Address = new NodeAddress("127.0.0.1", 6132);
            _n3Address = new NodeAddress("127.0.0.1", 6133);

            _addresses = new List<NodeAddress> { _n1Address, _n2Address, _n3Address };

            _node1 = new Node(_n1Address, _addresses);
            _node2 = new Node(_n2Address, _addresses);
            _node3 = new Node(_n3Address, _addresses);

            _state1.WireUpNode(_node1);
            _state2.WireUpNode(_node2);
            _state3.WireUpNode(_node3);

            _state1.LocalAdd("someData1");
            _state2.LocalAdd("someData2");
            _state3.LocalAdd("someData3");
            _state1.LocalAdd("someData4");
            _state2.LocalAdd("someData5");
            _state3.LocalAdd("someData6");
            _state1.LocalAdd("someData7");
            _state2.LocalAdd("someData8");
            _state3.LocalAdd("someData9");
            _state1.LocalAdd("someData10");

            Assert.That(_state1.GetLatestBlock().Transaction.GetData<string>(), Is.EqualTo("someData10"));
            Assert.That(_state2.GetLatestBlock().Transaction.GetData<string>(), Is.EqualTo("someData10"));
            Assert.That(_state3.GetLatestBlock().Transaction.GetData<string>(), Is.EqualTo("someData10"));
        }

        [Test]
        public async Task TestThatThreeNodesRemainSynchronisedAfter3KAddsToAllNodesInSequence()
        {
            _n1Address = new NodeAddress("127.0.0.1", 6141);
            _n2Address = new NodeAddress("127.0.0.1", 6142);
            _n3Address = new NodeAddress("127.0.0.1", 6143);

            _addresses = new List<NodeAddress> { _n1Address, _n2Address, _n3Address };

            _node1 = new Node(_n1Address, _addresses);
            _node2 = new Node(_n2Address, _addresses);
            _node3 = new Node(_n3Address, _addresses);

            _state1.WireUpNode(_node1);
            _state2.WireUpNode(_node2);
            _state3.WireUpNode(_node3);

            var dataCounter = 0;
            for (var i = 0; i < 1000; i++)
            {
                _state1.LocalAdd($"someData{dataCounter++}");
                _state2.LocalAdd($"someData{dataCounter++}");
                _state3.LocalAdd($"someData{dataCounter++}");
            }

            var expected = $"someData{dataCounter - 1}";

            Assert.That(_state1.GetLatestBlock().Transaction.GetData<string>(), Is.EqualTo(expected));
            Assert.That(_state2.GetLatestBlock().Transaction.GetData<string>(), Is.EqualTo(expected));
            Assert.That(_state3.GetLatestBlock().Transaction.GetData<string>(), Is.EqualTo(expected));
        }

        [Test]
        public async Task TestThatThreeNodesRemainSynchronisedAfter5KAddsToRandomNodes()
        {
            _n1Address = new NodeAddress("127.0.0.1", 6151);
            _n2Address = new NodeAddress("127.0.0.1", 6152);
            _n3Address = new NodeAddress("127.0.0.1", 6153);

            _addresses = new List<NodeAddress> { _n1Address, _n2Address, _n3Address };

            _node1 = new Node(_n1Address, _addresses);
            _node2 = new Node(_n2Address, _addresses);
            _node3 = new Node(_n3Address, _addresses);

            _state1.WireUpNode(_node1);
            _state2.WireUpNode(_node2);
            _state3.WireUpNode(_node3);

            var statePool = new List<AggregatedState<string>> { _state1, _state2, _state3 };
            var rand = new Random();

            var dataCounter = 0;
            for (var i = 0; i < 5000; i++)
            {
                var randomState = statePool[rand.Next(0, 2)];
                randomState.LocalAdd($"someData{dataCounter++}");
            }

            var expected = $"someData{dataCounter - 1}";

            Assert.That(_state1.GetLatestBlock().Transaction.GetData<string>(), Is.EqualTo(expected));
            Assert.That(_state2.GetLatestBlock().Transaction.GetData<string>(), Is.EqualTo(expected));
            Assert.That(_state3.GetLatestBlock().Transaction.GetData<string>(), Is.EqualTo(expected));
        }
    }

    

    //public interface IDataStrategy<T>
    //{
    //    T GetLastData();
    //    void Add(T dataToAdd);
    //}

    //public class DictionaryDataStrategy<T> : IDataStrategy<T>
    //{
    //    private Dictionary<string, T> _data = new Dictionary<string, T>();

    //    public T GetLastData()
    //    {
    //        return _data.Values.LastOrDefault();
    //    }

    //    public void Add(T dataToAdd)
    //    {
    //        _data.Add("", dataToAdd);
    //    }
    //}

    //public class ListDataStrategy<T> : IDataStrategy<T>
    //{
    //    private List<T> _data = new List<T>();

    //    public T GetLastData()
    //    {
    //        return _data.LastOrDefault();
    //    }

    //    public void Add(T dataToAdd)
    //    {
    //        _data.Add(dataToAdd);
    //    }
    //}
}