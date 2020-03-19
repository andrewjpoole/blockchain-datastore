using System;
using System.Collections.Generic;

namespace DataStore.BlockchainDB
{
    public interface IIndexedList<T>
    {
        IndexDefinition<T> AddIndex(string name, Func<T, string> keyPointer);
        bool Remove(T item);
        IEnumerable<T> FindBySingleIndex(IndexSearch search);
        IEnumerable<T> FindByIntersection(IndexSearch search1, IndexSearch search2);
        IEnumerable<T> FindByIndexEvaluateValues(IndexSearch search, Func<T, bool> predicate);
        void Add(T item);
    }
}