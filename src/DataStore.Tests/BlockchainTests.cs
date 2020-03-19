using AsyncInternals;
using FakeItEasy;
using NUnit.Framework;
using System;
using DataStore.Blockchain;

namespace DataStore.Tests
{
    [TestFixture]
    public class BlockchainTests
    {
        [Test]
        public void TestThatChainWithOneBlockIsValid()
        {
            var fakeDateTimeOffsetProvider = A.Fake<IDateTimeOffsetProvider>();
            A.CallTo(() => fakeDateTimeOffsetProvider.GetDateTimeOffset()).Returns(new DateTimeOffset(new DateTime(2018, 12, 01, 14, 32, 00))).Once()
                .Then.Returns(new DateTimeOffset(new DateTime(2018, 12, 01, 14, 33, 00))).Once()
                .Then.Returns(new DateTimeOffset(new DateTime(2018, 12, 01, 14, 34, 00))).Once();

            var chain = new Chain(fakeDateTimeOffsetProvider);

            chain.AddBlock(chain.CreateBlock(new Transaction("nodeA", "Add", "Id=sd7g8578s5g78d56s87g5")));

            Assert.That(chain.GetLatestBlock().Hash, Is.Not.Empty);
            Assert.That(chain.GetLatestBlock().Hash.Length, Is.EqualTo(44));
            Assert.That(chain.GetLatestBlock().Hash.Substring(0, 3), Is.EqualTo("YbT"));
            Assert.That(chain.Validate(), Is.EqualTo(true));
        }

        [Test]
        public void TestThatChainWithTwoBlocksIsValid()
        {
            var fakeDateTimeOffsetProvider = A.Fake<IDateTimeOffsetProvider>();
            A.CallTo(() => fakeDateTimeOffsetProvider.GetDateTimeOffset()).Returns(new DateTimeOffset(new DateTime(2018, 12, 01, 14, 32, 00))).Once()
                .Then.Returns(new DateTimeOffset(new DateTime(2018, 12, 01, 14, 33, 00))).Once()
                .Then.Returns(new DateTimeOffset(new DateTime(2018, 12, 01, 14, 34, 00))).Once();

            var chain = new Chain(fakeDateTimeOffsetProvider);

            chain.AddBlock(chain.CreateBlock(new Transaction("nodeA", "Add", "Id=sd7g8578s5g78d56s87g5")));
            chain.AddBlock(chain.CreateBlock(new Transaction("nodeA", "Add", "Id=d8976fd876f8d7689f76d")));

            Assert.That(chain.GetLatestBlock().Hash, Is.Not.Empty);
            Assert.That(chain.GetLatestBlock().Hash.Length, Is.EqualTo(44));
            Assert.That(chain.GetLatestBlock().Hash.Substring(0, 3), Is.EqualTo("jIG"));
            Assert.That(chain.GetPreviousBlock().Hash.Substring(0, 3), Is.EqualTo("jIG"));
            Assert.That(chain.Validate(), Is.EqualTo(true));
        }

        [Test]
        public void TestThatPreviousBlockWithOneBlockInChainReturnsGenesisBlock()
        {
            var fakeDateTimeOffsetProvider = A.Fake<IDateTimeOffsetProvider>();
            A.CallTo(() => fakeDateTimeOffsetProvider.GetDateTimeOffset()).Returns(new DateTimeOffset(new DateTime(2018, 12, 01, 14, 32, 00))).Once()
                .Then.Returns(new DateTimeOffset(new DateTime(2018, 12, 01, 14, 33, 00))).Once()
                .Then.Returns(new DateTimeOffset(new DateTime(2018, 12, 01, 14, 34, 00))).Once();

            var chain = new Chain(fakeDateTimeOffsetProvider);
            
            Assert.That(chain.GetPreviousBlock().Index, Is.EqualTo(0));
            Assert.That(chain.GetPreviousBlock().Hash.Substring(0, 3), Is.EqualTo("ngz"));
        }

        [Test]
        public void TestThatPreviousBlockWithTwoBlocksInChainReturnsPreviousBlock()
        {
            var fakeDateTimeOffsetProvider = A.Fake<IDateTimeOffsetProvider>();
            A.CallTo(() => fakeDateTimeOffsetProvider.GetDateTimeOffset()).Returns(new DateTimeOffset(new DateTime(2018, 12, 01, 14, 32, 00))).Once()
                .Then.Returns(new DateTimeOffset(new DateTime(2018, 12, 01, 14, 33, 00))).Once()
                .Then.Returns(new DateTimeOffset(new DateTime(2018, 12, 01, 14, 34, 00))).Once();

            var chain = new Chain(fakeDateTimeOffsetProvider);

            chain.AddBlock(chain.CreateBlock(new Transaction("nodeA", "Add", "Id=sd7g8578s5g78d56s87g5")));

            Assert.That(chain.GetPreviousBlock().Index, Is.EqualTo(1));
            Assert.That(chain.GetPreviousBlock().Hash.Substring(0, 3), Is.EqualTo("YbT"));
        }

        [Test]
        public void TestThatPreviousBlockWithThreeBlocksInChainReturnsPreviousBlock()
        {
            var fakeDateTimeOffsetProvider = A.Fake<IDateTimeOffsetProvider>();
            A.CallTo(() => fakeDateTimeOffsetProvider.GetDateTimeOffset()).Returns(new DateTimeOffset(new DateTime(2018, 12, 01, 14, 32, 00))).Once()
                .Then.Returns(new DateTimeOffset(new DateTime(2018, 12, 01, 14, 33, 00))).Once()
                .Then.Returns(new DateTimeOffset(new DateTime(2018, 12, 01, 14, 34, 00))).Once();

            var chain = new Chain(fakeDateTimeOffsetProvider);

            chain.AddBlock(chain.CreateBlock(new Transaction("nodeA", "Add", "Id=sd7g8578s5g78d56s87g5")));
            chain.AddBlock(chain.CreateBlock(new Transaction("nodeA", "Add", "Id=65ouybo6ouy45bo6yub56")));

            Assert.That(chain.GetPreviousBlock().Index, Is.EqualTo(2));
            Assert.That(chain.GetPreviousBlock().Hash.Substring(0, 3), Is.EqualTo("5WZ"));
        }

        [Test]
        public void TestThatValidateCanBeCalledUsingSubsetOfBlocks()
        {
            var fakeDateTimeOffsetProvider = A.Fake<IDateTimeOffsetProvider>();
            A.CallTo(() => fakeDateTimeOffsetProvider.GetDateTimeOffset()).Returns(new DateTimeOffset(new DateTime(2018, 12, 01, 14, 32, 00))).Once()
                .Then.Returns(new DateTimeOffset(new DateTime(2018, 12, 01, 14, 33, 00))).Once()
                .Then.Returns(new DateTimeOffset(new DateTime(2018, 12, 01, 14, 34, 00))).Once()
                .Then.Returns(new DateTimeOffset(new DateTime(2018, 12, 01, 14, 35, 00))).Once()
                .Then.Returns(new DateTimeOffset(new DateTime(2018, 12, 01, 14, 36, 00))).Once();

            var chain = new Chain(fakeDateTimeOffsetProvider);

            chain.AddBlock(chain.CreateBlock(new Transaction("nodeA", "Add", "Id=sd7g8578s5g78d56s87g5")));
            chain.AddBlock(chain.CreateBlock(new Transaction("nodeA", "Add", "Id=d8976fd876f8d7689f76d")));
            chain.AddBlock(chain.CreateBlock(new Transaction("nodeA", "Add", "Id=34j5hg45jhgjh5gjh4g5h")));
            chain.AddBlock(chain.CreateBlock(new Transaction("nodeA", "Add", "Id=684db68d4r6b84rd68bdd")));
                        
            Assert.That(chain.Validate(), Is.EqualTo(true));
            Assert.That(chain.Blocks.Count, Is.EqualTo(5));
            Assert.That(chain.Validate(100), Is.EqualTo(true));
            Assert.That(chain.Validate(5), Is.EqualTo(true));
            Assert.That(chain.Validate(4), Is.EqualTo(true));
            Assert.That(chain.Validate(3), Is.EqualTo(true));
            Assert.That(chain.Validate(2), Is.EqualTo(true));
            Assert.That(chain.Validate(1), Is.EqualTo(true));
        }
    }
}
