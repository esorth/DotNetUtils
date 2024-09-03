// .Net Utils - Misc utility classes/functions for use in .Net libraries.
// Written in 2024 by Eric Orth
//
// To the extent possible under law, the author(s) have dedicated all copyright and related and
// neighboring rights to this software to the public domain worldwide. This software is distributed
// without any warranty.
//
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If
// not, see http://creativecommons.org/publicdomain/zero/1.0/.

// Interfaces for dictionaries that also allow random access reads of elements by index.

using System.Collections;
using System.Collections.Immutable;

namespace DotNetUtils
{
    public interface IIndexedDictionary<TKey, TValue> : IReadOnlyIndexedDictionary<TKey, TValue>,
                                                        IDictionary<TKey, TValue>, IDictionary
    {
        public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items);
        public void RemoveRange(IEnumerable<TKey> keys);

        public void SetValueAt(Index index, TValue value);

        public void RemoveAt(Index index);
        public void RemoveAt(Range range);
    }

    public interface ISortedIndexedDictionary<TKey, TValue> :
        IIndexedDictionary<TKey, TValue>, IReadOnlySortedIndexedDictionary<TKey, TValue>
    { }

    public interface IReadOnlyIndexedDictionary<TKey, TValue> :
        IReadOnlyDictionary<TKey, TValue>, IReadOnlyList<KeyValuePair<TKey, TValue>>
    {
        public int IndexOf(TKey key);
        public int IndexOf(KeyValuePair<TKey, TValue> item);

        public KeyValuePair<TKey, TValue> At(Index index);
        public IIndexedDictionary<TKey, TValue> At(Range range);

        public bool TryGetKey(TKey key, out TKey actual);
    }

    public interface IReadOnlySortedIndexedDictionary<TKey, TValue> :
        IReadOnlyIndexedDictionary<TKey, TValue>
    {
        public IComparer<TKey> Comparer { get; }

        // Lower item is the greatest item that compares lower than and not equal to `key`.
        public KeyValuePair<TKey, TValue>? TryGetLowerItem(TKey key);
        public int? TryGetIndexOfLowerItem(TKey key);

        // Floor item is the greatest item that compares lower or equal to `key`.
        public KeyValuePair<TKey, TValue>? TryGetFloorItem(TKey key);
        public int? TryGetIndexOfFloorItem(TKey key);

        // Ceiling item is the lowest item that compares greater than or equal to `key`.
        public KeyValuePair<TKey, TValue>? TryGetCeilingItem(TKey key);
        public int? TryGetIndexOfCeilingItem(TKey key);

        // Higher item is the lowest item that compares higher than and not equal to `key`.
        public KeyValuePair<TKey, TValue>? TryGetHigherItem(TKey key);
        public int? TryGetIndexOfHigherItem(TKey key);

        public new ISortedIndexedDictionary<TKey, TValue> At(Range range);

        public ISortedIndexedDictionary<TKey, TValue> Slice(TKey first, TKey last);
    }

    public interface IImmutableIndexedDictionary<TKey, TValue> :
        IReadOnlyIndexedDictionary<TKey, TValue>, IImmutableDictionary<TKey, TValue>
    {
        public new IImmutableIndexedDictionary<TKey, TValue> At(Range range);

        public new IImmutableIndexedDictionary<TKey, TValue> Add(TKey key, TValue value);
        public new IImmutableIndexedDictionary<TKey, TValue> AddRange(
            IEnumerable<KeyValuePair<TKey, TValue>> items);
        public new IImmutableIndexedDictionary<TKey, TValue> Clear();
        public new IImmutableIndexedDictionary<TKey, TValue> Remove(TKey key);
        public new IImmutableIndexedDictionary<TKey, TValue> RemoveRange(
            IEnumerable<TKey> keys);
        public IImmutableIndexedDictionary<TKey, TValue> RemoveAt(Index index);
        public IImmutableIndexedDictionary<TKey, TValue> RemoveAt(Range range);
        public new IImmutableIndexedDictionary<TKey, TValue> SetItem(TKey key, TValue value);
        public new IImmutableIndexedDictionary<TKey, TValue> SetItems(
            IEnumerable<KeyValuePair<TKey, TValue>> items);
    }

    public interface IImmutableSortedIndexedDictionary<TKey, TValue> :
        IImmutableIndexedDictionary<TKey, TValue>, IReadOnlySortedIndexedDictionary<TKey, TValue>
    {
        public new IImmutableSortedIndexedDictionary<TKey, TValue> At(Range range);
        public new IImmutableSortedIndexedDictionary<TKey, TValue> Slice(TKey first, TKey last);

        public new IImmutableSortedIndexedDictionary<TKey, TValue> Add(TKey key, TValue value);
        public new IImmutableSortedIndexedDictionary<TKey, TValue> AddRange(
            IEnumerable<KeyValuePair<TKey, TValue>> items);
        public new IImmutableSortedIndexedDictionary<TKey, TValue> Clear();
        public new IImmutableSortedIndexedDictionary<TKey, TValue> Remove(TKey key);
        public new IImmutableSortedIndexedDictionary<TKey, TValue> RemoveRange(
            IEnumerable<TKey> keys);
        public new IImmutableSortedIndexedDictionary<TKey, TValue> RemoveAt(Index index);
        public new IImmutableSortedIndexedDictionary<TKey, TValue> RemoveAt(Range range);
        public new IImmutableSortedIndexedDictionary<TKey, TValue> SetItem(TKey key, TValue value);
        public new IImmutableSortedIndexedDictionary<TKey, TValue> SetItems(
            IEnumerable<KeyValuePair<TKey, TValue>> items);
    }
}
