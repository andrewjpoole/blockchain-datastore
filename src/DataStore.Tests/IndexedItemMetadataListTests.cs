using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DataStore.BlockchainDB;
using NUnit.Framework;

namespace DataStore.Tests
{
    [TestFixture]
    public class IndexedItemMetadataListTests
    {
        [Test]
        public void Test1()
        {
            var list = new IndexedList<ItemMetadata>();
            list.AddIndex("BlobRef", (item) => item.BlobRef);
            list.AddIndex("Name", (item) => item.Name);
            list.AddIndex("Value", (item) => item.Value);

            var rand = new Random(2654);

            var chances = new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 3 };
            var tags = new Dictionary<string, List<string>>();
            tags.Add("Tag", new List<string> { "Cuba", "pool", "beach", "kids" });
            tags.Add("Camera", new List<string> { "AndrewsPhone", "JosPhone", "LydiasPhone", "AllansCamera", "PoolesCamera", "JosPhone", "LydiasPhone", "AllansCamera", "PoolesCamera", "JosPhone", "LydiasPhone", "AllansCamera", "PoolesCamera" });

            var rememberedBlobRef = "";

            var sw = new Stopwatch();
            sw.Start();

            for (var i = 1; i <= 1_000_000; ++i)
            {
                var blobRef = Guid.NewGuid().ToString();
                list.Add(new ItemMetadata()
                {
                    BlobRef = blobRef,
                    Name = "BlobRef",
                    Value = $"{DateTime.UtcNow.AddDays(rand.Next(-10, 20)).AddHours(rand.Next(-20, 20))}"
                });

                // remember a random blobRef for lookup later
                if (i == 1000)
                    rememberedBlobRef = blobRef;

                var randomWeightedChance = chances[rand.Next(0, chances.Count)];
                for (var j = 0; j < randomWeightedChance; j++)
                {
                    var randomTag = tags.ElementAt(rand.Next(0, tags.Count));
                    var randomTagValue = randomTag.Value.ElementAt(rand.Next(0, randomTag.Value.Count));

                    list.Add(new ItemMetadata()
                    {
                        BlobRef = blobRef,
                        Name = randomTag.Key,
                        Value = randomTagValue
                    });
                }
            }
            
            var elapsed = sw.Elapsed.TotalMilliseconds;
            Debug.WriteLine($"list loaded {elapsed}ms");
            Assert.That(list.Count, Is.EqualTo(1_239_040));
            sw.Restart();

            var foundBlobRefMatches = list.FindBySingleIndex(new IndexSearch("BlobRef", rememberedBlobRef));
            elapsed = sw.Elapsed.TotalMilliseconds;
            Debug.WriteLine($"lookup via blobRef index should return at least 1 item in {elapsed}ms");
            Assert.That(foundBlobRefMatches.Count(), Is.EqualTo(1));
            sw.Restart();

            var allitemsWithNameTag = list.FindBySingleIndex(new IndexSearch("Name", "Tag"));
            elapsed = sw.Elapsed.TotalMilliseconds;
            Debug.WriteLine($"indexed search of all items with a Name:Tag in {elapsed}ms");
            Assert.That(allitemsWithNameTag.Count(), Is.EqualTo(119_587));
            sw.Restart();

            var allitemsWithNameAndValue = list.FindByIntersection(new IndexSearch("Name", "Camera"), new IndexSearch("Value", "AndrewsPhone"));
            elapsed = sw.Elapsed.TotalMilliseconds;
            Debug.WriteLine($"indexed search of all items with Name:Tag and Value: in {elapsed}ms");
            Assert.That(allitemsWithNameAndValue.Count(), Is.EqualTo(9321));
            sw.Restart();
            
            var allitemsWithNameBlobRef = list.FindByIndexEvaluateValues(new IndexSearch("Name", "BlobRef"), (v) => (DateTime.Parse(v.Value).Day == DateTime.UtcNow.Day));
            elapsed = sw.Elapsed.TotalMilliseconds;
            Debug.WriteLine($"indexed search via Name AND Value matching an evaluated predicate {elapsed}ms");
            Assert.That(allitemsWithNameBlobRef.Count(), Is.EqualTo(33_166));
        }

        [Test]
        public void TestAddRemoveAndClearMethods()
        {
            var sut = new IndexedList<ItemMetadata>();
            sut.AddIndex("Idx", metadata => metadata.BlobRef);

            var item1 = new ItemMetadata
            {
                BlobRef = Guid.NewGuid().ToString(),
                Name = "Tag",
                Value = "Cuba"
            };

            var item2 = new ItemMetadata
            {
                BlobRef = Guid.NewGuid().ToString(),
                Name = "Tag",
                Value = "beach"
            };

            var item3 = new ItemMetadata
            {
                BlobRef = Guid.NewGuid().ToString(),
                Name = "Tag",
                Value = "pooltable"
            };

            Assert.That(sut.Count, Is.EqualTo(0));

            sut.Add(item1);

            Assert.That(sut.Count, Is.EqualTo(1));

            sut.Add(item2);

            Assert.That(sut.Count, Is.EqualTo(2));

            sut.Insert(1, item3);
            Assert.That(sut.Count, Is.EqualTo(3));

            sut.RemoveAt(2);

            Assert.That(sut.Count, Is.EqualTo(2));
            Assert.That(sut[0].Value, Is.EqualTo("Cuba"));

            sut.Clear();

            Assert.That(sut.Count, Is.EqualTo(0));
        }
    }
}