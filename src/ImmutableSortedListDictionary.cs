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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace DotNetUtils
{
    // A list-based indexable dictionary. Allows efficient (generally O(lg(n)) access but slower
    // (generally O(n) or O(nlg(n))) modification.
    public class ImmutableSortedListDictionary<TKey, TValue> :
        IImmutableSortedIndexedDictionary<TKey, TValue>
        where TKey : notnull
    {
        public static ImmutableSortedListDictionary<TKey, TValue> Create()
        {
            return Create(keyComparer: null);
        }

        public static ImmutableSortedListDictionary<TKey, TValue> Create(
            IComparer<TKey>? keyComparer)
        {
            return new ImmutableSortedListDictionary<TKey, TValue>(
                new SortedList<TKey, TValue>(keyComparer));
        }

        public static ImmutableSortedListDictionary<TKey, TValue> CreateRange(
            IComparer<TKey> keyComparer, IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            if (items is ImmutableSortedListDictionary<TKey, TValue> sorted &&
                sorted._list.Comparer == keyComparer)
            {
                return new ImmutableSortedListDictionary<TKey, TValue>(sorted._list);
            }
            else
            {
                var dict = items.ToDictionary(item => item.Key, item => item.Value);
                return new ImmutableSortedListDictionary<TKey, TValue>(
                    new SortedList<TKey, TValue>(dict, keyComparer));
            }
        }

        public IComparer<TKey> Comparer => _list.Comparer;

        public int Count => _list.Count;

        public TValue this[TKey key] => _list[key];

        KeyValuePair<TKey, TValue> IReadOnlyList<KeyValuePair<TKey, TValue>>.this[int index]
            => At(index);

        public IEnumerable<TKey> Keys => _list.Keys;
        public IEnumerable<TValue> Values => _list.Values;

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _list.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool ContainsKey(TKey key) => _list.ContainsKey(key);
        public bool Contains(KeyValuePair<TKey, TValue> pair) => _list.Contains(pair);

        public KeyValuePair<TKey, TValue> At(Index index)
        {
            TKey key = _list.Keys[index];
            return new KeyValuePair<TKey, TValue>(key, _list[key]);
        }

        public ImmutableSortedListDictionary<TKey, TValue> At(Range range)
        {
            // TODO: Wrap like ImmutableListSlice to avoid copy.

            (int offset, int length) = range.GetOffsetAndLength(_list.Count);
            if (offset < 0 || offset + length > _list.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(range));
            }

            SortedList<TKey, TValue> copy = new(length, _list.Comparer);
            for (int i = offset; i < offset + length; i++)
            {
                TKey key = _list.Keys[i];
                copy.Add(key, _list[key]);
            }
            return new ImmutableSortedListDictionary<TKey, TValue>(copy);
        }

        IImmutableSortedIndexedDictionary<TKey, TValue>
            IImmutableSortedIndexedDictionary<TKey, TValue>.At(Range range)
        {
            return At(range);
        }

        IImmutableIndexedDictionary<TKey, TValue>
            IImmutableIndexedDictionary<TKey, TValue>.At(Range range)
        {
            return At(range);
        }

        ISortedIndexedDictionary<TKey, TValue>
            IReadOnlySortedIndexedDictionary<TKey, TValue>.At(Range range)
        {
            (int offset, int length) = range.GetOffsetAndLength(_list.Count);
            if (offset < 0 || offset + length > _list.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(range));
            }

            TreeIndexedDictionary<TKey, TValue> copy = new(Comparer);
            for (int i = offset; i < offset + length; i++)
            {
                copy.Add(At(i));
            }
            return copy;
        }

        public ImmutableSortedListDictionary<TKey, TValue> Slice(TKey first, TKey last)
        {
            if (Comparer.Compare(first, last) > 0)
            {
                throw new ArgumentException("First cannot compare after last.");
            }

            int i1 = IndexOf(first);
            int i2 = IndexOf(last);
            Debug.Assert(i1 <= i2);

            return At(i1..i2);
        }

        IImmutableSortedIndexedDictionary<TKey, TValue>
            IImmutableSortedIndexedDictionary<TKey, TValue>.Slice(TKey first, TKey last)
        {
            return Slice(first, last);
        }

        ISortedIndexedDictionary<TKey, TValue>
            IReadOnlySortedIndexedDictionary<TKey, TValue>.Slice(TKey first, TKey last)
        {
            if (Comparer.Compare(first, last) > 0)
            {
                throw new ArgumentException("First cannot compare after last.");
            }

            int i1 = IndexOf(first);
            int i2 = IndexOf(last);
            Debug.Assert(i1 <= i2);

            TreeIndexedDictionary<TKey, TValue> copy = new(Comparer);
            for (int i = i1; i <= i2; i++)
            {
                copy.Add(At(i));
            }
            return copy;
        }

        public int IndexOf(TKey key)
        {
            int index = _list.IndexOfKey(key);
            if (index >= 0)
            {
                return index;
            }
            else
            {
                throw new KeyNotFoundException();
            }
        }

        public int IndexOf(KeyValuePair<TKey, TValue> item)
        {
            if (_list.ContainsKey(item.Key) && Equals(_list[item.Key], item.Value))
            {
                return _list.IndexOfKey(item.Key);
            }
            else
            {
                throw new KeyNotFoundException();
            }
        }

        IIndexedDictionary<TKey, TValue> IReadOnlyIndexedDictionary<TKey, TValue>.At(Range range)
        {
            return (this as IReadOnlySortedIndexedDictionary<TKey, TValue>).At(range);
        }

        public KeyValuePair<TKey, TValue>? TryGetLowerItem(TKey key)
        {
            int? lower = TryGetIndexOfLowerItem(key);

            if (!lower.HasValue)
            {
                return null;
            }
            else
            {
                Debug.Assert(lower.Value >= 0 && lower.Value < _list.Count);
                TKey lowerKey = _list.Keys[lower.Value];
                Debug.Assert(_list.Comparer.Compare(lowerKey, key) < 0);
                return new KeyValuePair<TKey, TValue>(lowerKey, _list[lowerKey]);
            }
        }

        public int? TryGetIndexOfLowerItem(TKey key)
        {
            int? floor = TryGetIndexOfFloorItem(key);

            if (!floor.HasValue)
            {
                return null;
            }
            else if (_list.Comparer.Compare(_list.Keys[floor.Value], key) < 0)
            {
                return floor;
            }
            else if (floor.Value == 0)
            {
                return null;
            }
            else
            {
                return floor.Value - 1;
            }
        }

        public KeyValuePair<TKey, TValue>? TryGetFloorItem(TKey key)
        {
            int? floor = TryGetIndexOfFloorItem(key);

            if (!floor.HasValue)
            {
                return null;
            }
            else
            {
                Debug.Assert(floor.Value >= 0 && floor.Value < _list.Count);
                TKey floorKey = _list.Keys[floor.Value];
                Debug.Assert(_list.Comparer.Compare(floorKey, key) <= 0);
                return new KeyValuePair<TKey, TValue>(floorKey, _list[floorKey]);
            }
        }

        public int? TryGetIndexOfFloorItem(TKey key)
        {
            if (_list.Count == 0)
            {
                return null;
            }
            else if (_list.ContainsKey(key))
            {
                return _list.IndexOfKey(key);
            }

            int windowFirst = 0;
            int windowLast = _list.Count - 1;
            while (true)
            {
                int middle = (windowFirst + windowLast) / 2;

                Debug.Assert(_list.Comparer.Compare(_list.Keys[middle], key) != 0);
                if (_list.Comparer.Compare(key, _list.Keys[middle]) < 0)
                {
                    if (middle == 0)
                    {
                        Debug.Assert(windowFirst == 0);
                        return null;
                    }
                    else if (windowFirst == windowLast)
                    {
                        return middle - 1;
                    }
                    else
                    {
                        windowLast = middle - 1;
                        Debug.Assert(windowLast >= windowFirst);
                    }
                }
                else
                {
                    Debug.Assert(_list.Comparer.Compare(key, _list.Keys[middle]) > 0);

                    if (middle == _list.Count - 1 || windowFirst == windowLast)
                    {
                        return middle;
                    }
                    else
                    {
                        windowFirst = middle + 1;
                        Debug.Assert(windowFirst <= windowLast);
                    }
                }
            }
        }

        public KeyValuePair<TKey, TValue>? TryGetCeilingItem(TKey key)
        {
            int? ceiling = TryGetIndexOfCeilingItem(key);

            if (!ceiling.HasValue)
            {
                return null;
            }
            else
            {
                Debug.Assert(ceiling.Value >= 0 && ceiling.Value < _list.Count);
                TKey ceilingKey = _list.Keys[ceiling.Value];
                Debug.Assert(_list.Comparer.Compare(ceilingKey, key) >= 0);
                return new KeyValuePair<TKey, TValue>(ceilingKey, _list[ceilingKey]);
            }
        }

        public int? TryGetIndexOfCeilingItem(TKey key)
        {
            if (_list.Count == 0)
            {
                return null;
            }

            int? floor = TryGetIndexOfFloorItem(key);

            if (!floor.HasValue)
            {
                return 0;
            }
            else if (_list.Comparer.Compare(_list.Keys[floor.Value], key) == 0)
            {
                return floor;
            }
            else if (floor.Value + 1 < _list.Count)
            {
                return floor.Value + 1;
            }
            else
            {
                return null;
            }
        }

        public KeyValuePair<TKey, TValue>? TryGetHigherItem(TKey key)
        {
            int? higher = TryGetIndexOfHigherItem(key);

            if (!higher.HasValue)
            {
                return null;
            }
            else
            {
                Debug.Assert(higher.Value >= 0 && higher.Value < _list.Count);
                TKey higherKey = _list.Keys[higher.Value];
                Debug.Assert(_list.Comparer.Compare(higherKey, key) > 0);
                return new KeyValuePair<TKey, TValue>(higherKey, _list[higherKey]);
            }
        }

        public int? TryGetIndexOfHigherItem(TKey key)
        {
            int? ceiling = TryGetIndexOfCeilingItem(key);

            if (!ceiling.HasValue)
            {
                return null;
            }
            else if (_list.Comparer.Compare(_list.Keys[ceiling.Value], key) > 0)
            {
                return ceiling;
            }
            else if (ceiling.Value == _list.Count - 1)
            {
                return null;
            }
            else
            {
                return ceiling.Value + 1;
            }
        }

        public bool TryGetKey(TKey key, out TKey actual)
        {
            if (_list.ContainsKey(key))
            {
                actual = _list.Keys[_list.IndexOfKey(key)];
                return true;
            }
            else
            {
                actual = key;
                return false;
            }
        }

        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            return _list.TryGetValue(key, out value);
        }

        public ImmutableSortedListDictionary<TKey, TValue> Add(TKey key, TValue value)
        {
            if (_list.TryGetValue(key, out TValue? existing))
            {
                if (Equals(value, existing))
                {
                    return this;
                }
                else
                {
                    throw new ArgumentException(
                        "Dictionary already contains key " + key + " with different value.");
                }
            }
            else
            {
                ImmutableSortedListDictionary<TKey, TValue> copy = CreateRange(_list.Comparer, _list);
                copy._list.Add(key, value);
                return copy;
            }
        }

        IImmutableSortedIndexedDictionary<TKey, TValue>
            IImmutableSortedIndexedDictionary<TKey, TValue>.Add(TKey key, TValue value)
        {
            return Add(key, value);
        }

        IImmutableIndexedDictionary<TKey, TValue>
            IImmutableIndexedDictionary<TKey, TValue>.Add(TKey key, TValue value)
        {
            return Add(key, value);
        }

        IImmutableDictionary<TKey, TValue>
            IImmutableDictionary<TKey, TValue>.Add(TKey key, TValue value)
        {
            return Add(key, value);
        }

        public ImmutableSortedListDictionary<TKey, TValue>
            AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            var dict = items.ToDictionary(item => item.Key, item => item.Value);

            bool allFound = true;
            foreach (KeyValuePair<TKey, TValue> item in dict)
            {
                if (_list.TryGetValue(item.Key, out TValue? existing))
                {
                    if (!Equals(item.Value, existing))
                    {
                        throw new ArgumentException(
                            "Dictionary already contains key " + item.Key +
                            " with different value.");
                    }
                }
                else
                {
                    allFound = false;
                }
            }

            if (allFound)
            {
                return this;
            }

            ImmutableSortedListDictionary<TKey, TValue> copy = CreateRange(_list.Comparer, _list);
            foreach (KeyValuePair<TKey, TValue> item in dict)
            {
                if (!_list.ContainsKey(item.Key))
                {
                    copy._list.Add(item.Key, item.Value);
                }
            }
            return copy;
        }

        IImmutableSortedIndexedDictionary<TKey, TValue>
            IImmutableSortedIndexedDictionary<TKey, TValue>.AddRange(
                IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            return AddRange(items);
        }

        IImmutableIndexedDictionary<TKey, TValue>
            IImmutableIndexedDictionary<TKey, TValue>.AddRange(
                IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            return AddRange(items);
        }

        IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.AddRange(
            IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            return AddRange(items);
        }

        public ImmutableSortedListDictionary<TKey, TValue> Clear()
        {
            return Create(_list.Comparer);
        }

        IImmutableSortedIndexedDictionary<TKey, TValue>
            IImmutableSortedIndexedDictionary<TKey, TValue>.Clear()
        {
            return Clear();
        }

        IImmutableIndexedDictionary<TKey, TValue>
            IImmutableIndexedDictionary<TKey, TValue>.Clear()
        {
            return Clear();
        }

        IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.Clear()
        {
            return Clear();
        }

        public ImmutableSortedListDictionary<TKey, TValue> Remove(TKey key)
        {
            if (_list.ContainsKey(key))
            {
                ImmutableSortedListDictionary<TKey, TValue> copy = CreateRange(_list.Comparer, _list);
                bool removed = copy._list.Remove(key);
                Debug.Assert(removed);
                return copy;
            }
            else
            {
                return this;
            }
        }

        IImmutableSortedIndexedDictionary<TKey, TValue>
            IImmutableSortedIndexedDictionary<TKey, TValue>.Remove(TKey key)
        {
            return Remove(key);
        }

        IImmutableIndexedDictionary<TKey, TValue>
            IImmutableIndexedDictionary<TKey, TValue>.Remove(TKey key)
        {
            return Remove(key);
        }

        IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.Remove(TKey key)
        {
            return Remove(key);
        }

        public ImmutableSortedListDictionary<TKey, TValue> RemoveRange(IEnumerable<TKey> keys)
        {
            if (keys is null)
            {
                throw new ArgumentNullException(nameof(keys));
            }

            bool anyRemoved = false;
            ImmutableSortedListDictionary<TKey, TValue> copy = CreateRange(_list.Comparer, _list);
            foreach (TKey key in keys)
            {
                if (copy._list.Remove(key))
                {
                    anyRemoved = true;
                }
            }

            if (anyRemoved)
            {
                return copy;
            }
            else
            {
                return this;
            }
        }

        IImmutableSortedIndexedDictionary<TKey, TValue>
            IImmutableSortedIndexedDictionary<TKey, TValue>.RemoveRange(IEnumerable<TKey> keys)
        {
            return RemoveRange(keys);
        }

        IImmutableIndexedDictionary<TKey, TValue>
            IImmutableIndexedDictionary<TKey, TValue>.RemoveRange(IEnumerable<TKey> keys)
        {
            return RemoveRange(keys);
        }

        IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.RemoveRange(
            IEnumerable<TKey> keys)
        {
            return RemoveRange(keys);
        }

        public ImmutableSortedListDictionary<TKey, TValue> RemoveAt(Index index)
        {
            int offset = index.GetOffset(_list.Count);
            if (offset < 0 || offset >= _list.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            else
            {
                ImmutableSortedListDictionary<TKey, TValue> copy = CreateRange(_list.Comparer, _list);
                bool removed = copy._list.Remove(_list.Keys[offset]);
                Debug.Assert(removed);
                return copy;
            }
        }

        IImmutableSortedIndexedDictionary<TKey, TValue>
            IImmutableSortedIndexedDictionary<TKey, TValue>.RemoveAt(Index index)
        {
            return RemoveAt(index);
        }

        IImmutableIndexedDictionary<TKey, TValue>
            IImmutableIndexedDictionary<TKey, TValue>.RemoveAt(Index index)
        {
            return RemoveAt(index);
        }

        public ImmutableSortedListDictionary<TKey, TValue> RemoveAt(Range range)
        {
            (int offset, int length) = range.GetOffsetAndLength(Count);
            if (offset < 0 || offset + length > Count)
            {
                throw new ArgumentOutOfRangeException(nameof(range));
            }

            List<TKey> toRemove = new(length);
            for (int i = offset; i < offset + length; i++)
            {
                toRemove.Add(At(i).Key);
            }

            return RemoveRange(toRemove);
        }

        IImmutableSortedIndexedDictionary<TKey, TValue>
            IImmutableSortedIndexedDictionary<TKey, TValue>.RemoveAt(Range range)
        {
            return RemoveAt(range);
        }

        IImmutableIndexedDictionary<TKey, TValue>
            IImmutableIndexedDictionary<TKey, TValue>.RemoveAt(Range range)
        {
            return RemoveAt(range);
        }

        public ImmutableSortedListDictionary<TKey, TValue> SetItem(TKey key, TValue value)
        {
            if (TryGetValue(key, out TValue? existing) && Equals(value, existing))
            {
                return this;
            }
            else
            {
                ImmutableSortedListDictionary<TKey, TValue> copy = CreateRange(_list.Comparer, _list);
                copy._list[key] = value;
                return copy;
            }
        }

        IImmutableSortedIndexedDictionary<TKey, TValue>
            IImmutableSortedIndexedDictionary<TKey, TValue>.SetItem(TKey key, TValue value)
        {
            return SetItem(key, value);
        }

        IImmutableIndexedDictionary<TKey, TValue>
            IImmutableIndexedDictionary<TKey, TValue>.SetItem(TKey key, TValue value)
        {
            return SetItem(key, value);
        }

        IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.SetItem(
            TKey key, TValue value)
        {
            return SetItem(key, value);
        }

        public ImmutableSortedListDictionary<TKey, TValue> SetItems(
            IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            ImmutableSortedListDictionary<TKey, TValue> copy = CreateRange(_list.Comparer, _list);
            bool anyModified = false;
            foreach (KeyValuePair<TKey, TValue> item in items)
            {
                if (!TryGetValue(item.Key, out TValue? existing) || !Equals(item.Value, existing))
                {
                    anyModified = true;
                    copy._list[item.Key] = item.Value;
                }
            }

            if (anyModified)
            {
                return copy;
            }
            else
            {
                return this;
            }
        }

        IImmutableSortedIndexedDictionary<TKey, TValue>
            IImmutableSortedIndexedDictionary<TKey, TValue>.SetItems(
                IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            return SetItems(items);
        }

        IImmutableIndexedDictionary<TKey, TValue>
            IImmutableIndexedDictionary<TKey, TValue>.SetItems(
                IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            return SetItems(items);
        }

        IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.SetItems(
           IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            return SetItems(items);
        }

        private ImmutableSortedListDictionary(SortedList<TKey, TValue> list)
        {
            _list = list;
        }

        private readonly SortedList<TKey, TValue> _list;
    }
}
