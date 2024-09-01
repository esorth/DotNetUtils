// .Net Utils - Misc utility classes/functions for use in .Net libraries.
// Written in 2024 by Eric Orth
//
// To the extent possible under law, the author(s) have dedicated all copyright and related and
// neighboring rights to this software to the public domain worldwide. This software is distributed
// without any warranty.
//
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If
// not, see http://creativecommons.org/publicdomain/zero/1.0/.

using System.Collections;
using System.Collections.Immutable;

namespace DotNetUtils
{
    public interface IIndexedDictionary<TKey, TValue> : IReadOnlyIndexedDictionary<TKey, TValue>,
                                                        IDictionary<TKey, TValue>, IDictionary
    {
        public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items);

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
    { }

    public interface IImmutableSortedIndexedDictionary<TKey, TValue> :
        IImmutableIndexedDictionary<TKey, TValue>, IReadOnlySortedIndexedDictionary<TKey, TValue>
    { }
}
