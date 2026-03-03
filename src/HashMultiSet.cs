// .Net Utils - Misc utility classes/functions for use in .Net libraries.
// Written in 2026 by Eric Orth
//
// To the extent possible under law, the author(s) have dedicated all copyright and related and
// neighboring rights to this software to the public domain worldwide. This software is distributed
// without any warranty.
//
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If
// not, see http://creativecommons.org/publicdomain/zero/1.0/.

using System.Collections;
using System.Diagnostics;

namespace DotNetUtils
{
    // A multi-set implemented using hash tables for fast (typically O(1)) but unordered access and
    // modification.
    public class HashMultiSet<T>(IEqualityComparer<T>? comparer) : IMultiSet<T> where T : notnull
    {
        private readonly Dictionary<T, int> _counts = new(comparer);
        private int _totalCount = 0;

        public HashMultiSet() : this((IEqualityComparer<T>?)null) { }

        public HashMultiSet(IEnumerable<T> items) : this((IEqualityComparer<T>?)null)
        {
            ArgumentNullException.ThrowIfNull(items);
            AddRange(items);
        }

        public HashMultiSet(IEnumerable<T> items, IEqualityComparer<T>? comparer) : this(comparer)
        {
            ArgumentNullException.ThrowIfNull(items);
            AddRange(items);
        }

        // ICollection<T>.Add(T)
        public void Add(T item) => AddN(item, 1);
        public void AddN(T item, int count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (count == 0) return;

            if (_counts.TryGetValue(item, out int existing))
                _counts[item] = existing + count;
            else
                _counts[item] = count;

            _totalCount += count;
        }
        public void AddRange(IEnumerable<T> items)
        {
            ArgumentNullException.ThrowIfNull(items);
            foreach (T item in items) Add(item);
        }

        public bool RemoveOne(T item) => RemoveN(item, 1) > 0;
        public int RemoveN(T item, int count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (count == 0) return 0;

            if (!_counts.TryGetValue(item, out int existing) || existing == 0) return 0;

            int removed = existing <= count ? existing : count;
            if (removed == existing)
            {
                _counts.Remove(item);
            }
            else
            {
                _counts[item] = existing - removed;
                Debug.Assert(_counts[item] > 0);
            }

            _totalCount -= removed;
            return removed;
        }
        public int Remove(T item)
        {
            if (!_counts.TryGetValue(item, out int count) || count == 0) return 0;
            _counts.Remove(item);
            _totalCount -= count;
            return count;
        }
        // Expected to remove a single instance, so implement using RemoveOne().
        bool ICollection<T>.Remove(T item) => RemoveOne(item);

        public int RemoveOneRange(IEnumerable<T> items)
        {
            ArgumentNullException.ThrowIfNull(items);
            int removed = 0;
            foreach (T item in items) if (RemoveOne(item)) removed++;
            return removed;
        }

        // Per IMultiSet<T>, removes all matching values for each item in `items`. To remove just a
        // single instance of each item in `items`, use RemoveOneRange().
        public int RemoveRange(IEnumerable<T> items)
        {
            ArgumentNullException.ThrowIfNull(items);
            int removed = 0;
            foreach (T item in items) removed += Remove(item);
            return removed;
        }

        public void SetCount(T item, int count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));

            if (count == 0)
            {
                Remove(item);
            }
            else if (_counts.TryGetValue(item, out int prev))
            {
                _counts[item] = count;
                _totalCount += count - prev;
            }
            else
            {
                _counts[item] = count;
                _totalCount += count;
            }
        }

        // ICollection<T>.Clear()
        public void Clear()
        {
            _counts.Clear();
            _totalCount = 0;
        }

        // ICollection<T>.CopyTo(T[], int)
        public void CopyTo(T[] array, int arrayIndex)
        {
            ArgumentNullException.ThrowIfNull(array);
            if (arrayIndex < 0 || arrayIndex > array.Length)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (array.Length - arrayIndex < _totalCount)
                throw new ArgumentException("Destination array is too small.");

            int idx = arrayIndex;
            foreach (KeyValuePair<T, int> pair in _counts)
            {
                for (int i = 0; i < pair.Value; i++)
                    array[idx++] = pair.Key;
            }
        }

        // ICollection<T>.Count
        public int Count => _totalCount;
        public int CountUnique => _counts.Count;
        int IReadOnlyCollection<IReadOnlyCollection<T>>.Count => CountUnique;
        int IReadOnlyCollection<KeyValuePair<T, int>>.Count => CountUnique;

        public IReadOnlySet<IReadOnlyCollection<T>> ItemCollections => this;
        public IReadOnlySet<T> UniqueItems => new UniqueItemsView(this);
        public IReadOnlyDictionary<T, int> ItemCounts => this;
        // IReadOnlyDictionary<T, int>.Keys
        public IEnumerable<T> Keys => _counts.Keys;
        // IReadOnlyDictionary<T, int>.Values
        public IEnumerable<int> Values => _counts.Values;

        // ICollection<T>.IsReadOnly
        public bool IsReadOnly => false;

        // IReadOnlyDictionary<T, int>.this[TKey]
        public int this[T key]
        {
            get
            {
                if (_counts.TryGetValue(key, out int v)) return v;
                throw new KeyNotFoundException();
            }
        }

        public int CountOf(T item)
        {
            ArgumentNullException.ThrowIfNull(item);
            return _counts.TryGetValue(item, out int count) ? count : 0;
        }

        public bool Contains(T item)
        {
            ArgumentNullException.ThrowIfNull(item);
            return _counts.ContainsKey(item);
        }

        public bool Contains(T item, int count)
        {
            ArgumentNullException.ThrowIfNull(item);
            if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));
            return CountOf(item) == count;
        }

        // IReadOnlySet<IReadOnlyCollection<T>>.Contains(T1)
        public bool Contains(IReadOnlyCollection<T>? items)
        {
            if (items == null || items.Count == 0) return false;

            return ValidateDuplicateGrouping(items) &&
                _counts.TryGetValue(items.First(), out int count) &&
                count == items.Count;
        }

        // IReadOnlyDictionary<T, int>.ContainsKey(TKey)
        public bool ContainsKey(T key) => _counts.ContainsKey(key);

        // IReadOnlyDictionary<T, int>.TryGetValue(TKey, out TValue)
        public bool TryGetValue(T key, out int value) => _counts.TryGetValue(key, out value);

        public IEnumerator<T> GetEnumerator()
        {
            foreach (KeyValuePair<T, int> pair in _counts)
            {
                for (int i = 0; i < pair.Value; i++)
                    yield return pair.Key;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        IEnumerator<KeyValuePair<T, int>> IEnumerable<KeyValuePair<T, int>>.GetEnumerator()
        {
            return _counts.GetEnumerator();
        }

        IEnumerator<IReadOnlyCollection<T>> IEnumerable<IReadOnlyCollection<T>>.GetEnumerator()
        {
            foreach (KeyValuePair<T, int> pair in _counts)
            {
                yield return Enumerable.Repeat(pair.Key, pair.Value).ToArray();
            }
        }

        // IReadOnlySet<IReadOnlyCollection<T>>.SetEquals(IEnumerable<T1>)
        public bool SetEquals(IEnumerable<IReadOnlyCollection<T>> other)
        {
            ArgumentNullException.ThrowIfNull(other);
            HashSet<KeyValuePair<T, int>> ours =
                new(_counts, new KeyValuePairComparer(_counts.Comparer));
            HashSet<KeyValuePair<T, int>> theirs = ToPairSet(other);
            return ours.SetEquals(theirs);
        }

        // IReadOnlySet<IReadOnlyCollection<T>>.IsSubsetOf(IEnumerable<T1>)
        public bool IsSubsetOf(IEnumerable<IReadOnlyCollection<T>> other)
        {
            ArgumentNullException.ThrowIfNull(other);
            HashSet<KeyValuePair<T, int>> ours =
                new(_counts, new KeyValuePairComparer(_counts.Comparer));
            HashSet<KeyValuePair<T, int>> theirs = ToPairSet(other);
            return ours.IsSubsetOf(theirs);
        }

        // IReadOnlySet<IReadOnlyCollection<T>>.IsProperSubsetOf(IEnumerable<T1>)
        public bool IsProperSubsetOf(IEnumerable<IReadOnlyCollection<T>> other)
        {
            ArgumentNullException.ThrowIfNull(other);
            HashSet<KeyValuePair<T, int>> ours =
                new(_counts, new KeyValuePairComparer(_counts.Comparer));
            HashSet<KeyValuePair<T, int>> theirs = ToPairSet(other);
            return ours.IsProperSubsetOf(theirs);
        }

        // IReadOnlySet<IReadOnlyCollection<T>>.IsSupersetOf(IEnumerable<T1>)
        public bool IsSupersetOf(IEnumerable<IReadOnlyCollection<T>> other)
        {
            ArgumentNullException.ThrowIfNull(other);
            HashSet<KeyValuePair<T, int>> ours =
                new(_counts, new KeyValuePairComparer(_counts.Comparer));
            HashSet<KeyValuePair<T, int>> theirs = ToPairSet(other);
            return ours.IsSupersetOf(theirs);
        }

        // IReadOnlySet<IReadOnlyCollection<T>>.IsProperSupersetOf(IEnumerable<T1>)
        public bool IsProperSupersetOf(IEnumerable<IReadOnlyCollection<T>> other)
        {
            ArgumentNullException.ThrowIfNull(other);
            HashSet<KeyValuePair<T, int>> ours =
                new(_counts, new KeyValuePairComparer(_counts.Comparer));
            HashSet<KeyValuePair<T, int>> theirs = ToPairSet(other);
            return ours.IsProperSupersetOf(theirs);
        }

        // IReadOnlySet<IReadOnlyCollection<T>>.Overlaps(IEnumerable<T1>)
        public bool Overlaps(IEnumerable<IReadOnlyCollection<T>> other)
        {
            ArgumentNullException.ThrowIfNull(other);
            HashSet<KeyValuePair<T, int>> ours =
                new(_counts, new KeyValuePairComparer(_counts.Comparer));
            HashSet<KeyValuePair<T, int>> theirs = ToPairSet(other);
            return ours.Overlaps(theirs);
        }

        private bool ValidateDuplicateGrouping(IReadOnlyCollection<T>? items)
        {
            if (items == null || items.Count == 0) return false;

            T first = items.First();
            return items.All(item => _counts.Comparer.Equals(item, first));
        }

        // Set operations implemented by materializing sets of (element,count) pairs
        private HashSet<KeyValuePair<T, int>> ToPairSet(IEnumerable<IReadOnlyCollection<T>> source)
        {
            var set = new HashSet<KeyValuePair<T, int>>(new KeyValuePairComparer(_counts.Comparer));
            foreach (IReadOnlyCollection<T> collection in source)
            {
                if (ValidateDuplicateGrouping(collection))
                {
                    set.Add(new KeyValuePair<T, int>(collection.First(), collection.Count));
                }
                else
                {
                    // If the collection isn't a valid grouping of duplicates, add a dummy pair that
                    // won't match any valid pair in `_counts` (because `_counts` never contains
                    // a pair with a count of -1, even if `default` is potentially valid). This will
                    // allow logic to do the right thing whatever the operation will do with
                    // non-matching data in the input set.
                    set.Add(new KeyValuePair<T, int>(default!, -1));
                }
            }
            return set;
        }

        private sealed class KeyValuePairComparer(IEqualityComparer<T> cmp)
        : IEqualityComparer<KeyValuePair<T, int>>
        {
            private readonly IEqualityComparer<T> _cmp = cmp;

            public bool Equals(KeyValuePair<T, int> x, KeyValuePair<T, int> y)
            {
                if (x.Value < 0 || y.Value < 0)
                {
                    // If either pair has a negative count, it's a dummy pair that should never
                    // match any valid pair in `_counts`, even if `default` is a valid key.
                    // Consider all dummy pairs to be equal to each other (so that a HashSet can
                    // just store one dummy pair for any number of invalid input collections). Avoid
                    // passing the dummy key to `_cmp` in case it has issues with `default`.
                    return x.Value < 0 && y.Value < 0;
                }
                else
                {
                    return _cmp.Equals(x.Key, y.Key) && x.Value == y.Value;
                }
            }

            public int GetHashCode(KeyValuePair<T, int> obj)
            {
                if (obj.Value < 0)
                {
                    // All dummy pairs are considered equivalent and should hash to the same value.
                    return -1.GetHashCode();
                }
                else
                {
                    return HashCode.Combine(_cmp.GetHashCode(obj.Key), obj.Value);
                }
            }
        }

        private sealed class UniqueItemsView(HashMultiSet<T> parent) : IReadOnlySet<T>
        {
            private readonly HashMultiSet<T> _parent = parent;

            public int Count => _parent._counts.Count;
            public bool Contains(T item) => _parent._counts.ContainsKey(item);
            public IEnumerator<T> GetEnumerator() => _parent._counts.Keys.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public bool IsProperSubsetOf(IEnumerable<T> other) => ToSet().IsProperSubsetOf(other);
            public bool IsProperSupersetOf(IEnumerable<T> other)
            {
                return ToSet().IsProperSupersetOf(other);
            }
            public bool IsSubsetOf(IEnumerable<T> other) => ToSet().IsSubsetOf(other);
            public bool IsSupersetOf(IEnumerable<T> other) => ToSet().IsSupersetOf(other);
            public bool Overlaps(IEnumerable<T> other) => ToSet().Overlaps(other);
            public bool SetEquals(IEnumerable<T> other) => ToSet().SetEquals(other);

            private HashSet<T> ToSet() => new(_parent._counts.Keys, _parent._counts.Comparer);
        }
    }
}
