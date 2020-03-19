using System;
using System.Collections.Generic;
using System.Linq;

namespace DataStore.BlockchainDB
{
    public class IndexedList<T> : List<T>, IIndexedList<T>
    {
        private List<IndexDefinition<T>> _indicies = new List<IndexDefinition<T>>();

        public IndexDefinition<T> AddIndex(string name, Func<T, string> keyPointer)
        {
            var def = new IndexDefinition<T>
            {
                Name = name,
                KeyPointer = keyPointer,
                Dict = new Dictionary<string, ICollection<T>>()
            };

            _indicies.Add(def);
            return def;
        }

        public new void Add(T item)
        {
            base.Add(item);

            UpdateIndicies(item);
        }

        void UpdateIndicies(T item)
        {
            foreach (var indexDef in _indicies)
            {
                var index = indexDef.Dict;
                var keyPointer = indexDef.KeyPointer(item);
                if (index.ContainsKey(keyPointer))
                {
                    index[keyPointer].Add(item);
                }
                else
                {
                    index[keyPointer] = new List<T> { item };
                }
            }
        }

        public new void Clear()
        {
            base.Clear();
            foreach (var indexDef in _indicies)
            {
                indexDef.Dict.Clear();
            }
        }

        public new void Insert(int index, T item)
        {
            base.Insert(index, item);

            UpdateIndicies(item);
        }

        public new bool Remove(T item)
        {
            foreach (var indexDef in _indicies)
            {
                indexDef.Dict[indexDef.KeyPointer(item)]?.Remove(item);
            }

            base.Remove(item);

            return true;
        }

        public new void RemoveAt(int index)
        {
            var item = base[index];

            Remove(item);
        }

        public IEnumerable<T> FindBySingleIndex(IndexSearch search)
        {
            return _indicies.FirstOrDefault(x => x.Name == search.IndexName).Dict[search.ValueToMatch];
        }

        public IEnumerable<T> FindByIntersection(IndexSearch search1, IndexSearch search2)
        {
            var index1Matches = _indicies.FirstOrDefault(x => x.Name == search1.IndexName).Dict[search1.ValueToMatch];
            var index2Matches = _indicies.FirstOrDefault(x => x.Name == search2.IndexName).Dict[search2.ValueToMatch];

            return index1Matches.Intersect(index2Matches);
        }

        public IEnumerable<T> FindByIndexEvaluateValues(IndexSearch search, Func<T, bool> predicate)
        {
            var itemsMatchingValue = _indicies.FirstOrDefault(x => x.Name == search.IndexName).Dict[search.ValueToMatch];
            var matchingItems = new List<T>();
            foreach (var item in itemsMatchingValue)
            {
                if (predicate(item))
                    matchingItems.Add(item);
            }
            return matchingItems;
        }
    }

    public class IndexDefinition<T>
    {
        public string Name { get; set; }
        public Dictionary<string, ICollection<T>> Dict { get; set; }
        public Func<T, string> KeyPointer { get; set; }
    }

    public class IndexSearch
    {
        public string IndexName { get; set; }
        public string ValueToMatch { get; set; }

        public IndexSearch(string name, string value)
        {
            IndexName = name;
            ValueToMatch = value;
        }
    }
}