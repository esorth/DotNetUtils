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
using System.Collections.Immutable;
using System.Diagnostics;

namespace DotNetUtils;

public static class ImmutableHashMultiSet
{
    public static ImmutableHashMultiSet<T> Empty<T>() where T : notnull
    {
        return ImmutableHashMultiSet<T>.Empty;
    }

    public static ImmutableHashMultiSet<T> Create<T>() where T : notnull
    {
        return ImmutableHashMultiSet<T>.Empty;
    }

    public static ImmutableHashMultiSet<T> Create<T>(IEqualityComparer<T>? keyComparer)
    where T : notnull
    {
        return ImmutableHashMultiSet<T>.Empty.WithComparer(keyComparer);
    }

    public static ImmutableHashMultiSet<T> CreateRange<T>(IEnumerable<T> items) where T : notnull
    {
        return ImmutableHashMultiSet<T>.Empty.AddRange(items);
    }

    public static ImmutableHashMultiSet<T> CreateRange<T>(
        IEqualityComparer<T>? keyComparer, IEnumerable<T> items)
    where T : notnull
    {
        return ImmutableHashMultiSet<T>.Empty.WithComparer(keyComparer).AddRange(items);
    }

    public static ImmutableHashMultiSet<T>.Builder CreateBuilder<T>() where T : notnull
    {
        return Create<T>().ToBuilder();
    }

    public static ImmutableHashMultiSet<T>.Builder CreateBuilder<T>(
        IEqualityComparer<T>? keyComparer)
    where T : notnull
    {
        return Create<T>(keyComparer).ToBuilder();
    }
}

public sealed class ImmutableHashMultiSet<T> : IImmutableMultiSet<T> where T : notnull
{
    public static readonly ImmutableHashMultiSet<T> Empty =
        new(ImmutableDictionary<T, int>.Empty, 0);

    private readonly ImmutableDictionary<T, int> _counts;
    private readonly int _totalCount;

    internal ImmutableHashMultiSet(ImmutableDictionary<T, int> counts, int totalCount)
    {
        _counts = counts;
        _totalCount = totalCount;
    }

    public ImmutableHashMultiSet<T> WithComparer(IEqualityComparer<T>? comparer)
    {
        if (_counts.KeyComparer == comparer) return this;
        return new ImmutableHashMultiSet<T>(_counts.WithComparers(comparer), _totalCount);
    }

    public ImmutableHashMultiSet<T> Add(T item) => AddN(item, 1);
    IImmutableMultiSet<T> IImmutableMultiSet<T>.Add(T item) => Add(item);
    IImmutableCollection<T> IImmutableCollection<T>.Add(T item) => Add(item);

    public ImmutableHashMultiSet<T> AddN(T item, int count)
    {
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
        if (count == 0) return this;

        if (_counts.TryGetValue(item, out int existing))
        {
            return new ImmutableHashMultiSet<T>(
                _counts.SetItem(item, existing + count), _totalCount + count);
        }
        else
        {
            return new ImmutableHashMultiSet<T>(_counts.Add(item, count), _totalCount + count);
        }
    }
    IImmutableMultiSet<T> IImmutableMultiSet<T>.AddN(T item, int count) => AddN(item, count);

    public ImmutableHashMultiSet<T> AddRange(IEnumerable<T> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        if (items is ICollection<T> c && c.Count == 0) return this;

        var builder = _counts.ToBuilder();
        int totalCount = _totalCount;
        foreach (T item in items)
        {
            if (builder.TryGetValue(item, out int existing))
            {
                builder[item] = existing + 1;
            }
            else
            {
                builder[item] = 1;
            }
            totalCount++;
        }
        return new ImmutableHashMultiSet<T>(builder.ToImmutable(), totalCount);
    }
    IImmutableMultiSet<T> IImmutableMultiSet<T>.AddRange(IEnumerable<T> items) => AddRange(items);

    public ImmutableHashMultiSet<T> RemoveOne(T item) => RemoveN(item, 1);
    IImmutableMultiSet<T> IImmutableMultiSet<T>.RemoveOne(T item) => RemoveOne(item);

    public ImmutableHashMultiSet<T> RemoveN(T item, int count)
    {
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
        if (count == 0) return this;

        if (!_counts.TryGetValue(item, out int existing)) return this;

        int removed = Math.Min(existing, count);
        Debug.Assert(removed > 0);

        if (removed == existing)
        {
            return new ImmutableHashMultiSet<T>(_counts.Remove(item), _totalCount - removed);
        }
        else
        {
            return new ImmutableHashMultiSet<T>(_counts.SetItem(
                item, existing - removed), _totalCount - removed);
        }
    }
    IImmutableMultiSet<T> IImmutableMultiSet<T>.RemoveN(T item, int count) => RemoveN(item, count);

    public ImmutableHashMultiSet<T> Remove(T item)
    {
        if (_counts.TryGetValue(item, out int existing))
        {
            return new ImmutableHashMultiSet<T>(_counts.Remove(item), _totalCount - existing);
        }
        else
        {
            return this;
        }
    }
    IImmutableMultiSet<T> IImmutableMultiSet<T>.Remove(T item) => Remove(item);
    // IImmutableCollection<T>.Remove() is defined to only remove one instance, not all matching.
    IImmutableCollection<T> IImmutableCollection<T>.Remove(T item) => RemoveOne(item);

    public ImmutableHashMultiSet<T> RemoveOneRange(IEnumerable<T> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        var builder = _counts.ToBuilder();
        int totalCount = _totalCount;
        bool anyRemoved = false;
        foreach (T item in items)
        {
            if (builder.TryGetValue(item, out int existing))
            {
                if (existing == 1)
                {
                    builder.Remove(item);
                }
                else
                {
                    builder[item] = existing - 1;
                }
                totalCount--;
                anyRemoved = true;
            }
        }

        if (anyRemoved)
        {
            return new ImmutableHashMultiSet<T>(builder.ToImmutable(), totalCount);
        }
        else
        {
            return this;
        }
    }
    IImmutableMultiSet<T> IImmutableMultiSet<T>.RemoveOneRange(IEnumerable<T> items)
    {
        return RemoveOneRange(items);
    }

    public ImmutableHashMultiSet<T> RemoveRange(IEnumerable<T> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        var builder = _counts.ToBuilder();
        int totalCount = _totalCount;
        bool anyRemoved = false;
        foreach (T item in items)
        {
            if (builder.TryGetValue(item, out int existing))
            {
                builder.Remove(item);
                totalCount -= existing;
                anyRemoved = true;
            }
        }

        if (anyRemoved)
        {
            return new ImmutableHashMultiSet<T>(builder.ToImmutable(), totalCount);
        }
        else
        {
            return this;
        }
    }
    IImmutableMultiSet<T> IImmutableMultiSet<T>.RemoveRange(IEnumerable<T> items)
    {
        return RemoveRange(items);
    }

    public ImmutableHashMultiSet<T> Clear() => Empty.WithComparer(_counts.KeyComparer);
    IImmutableMultiSet<T> IImmutableMultiSet<T>.Clear() => Clear();
    IImmutableCollection<T> IImmutableCollection<T>.Clear() => Clear();

    public ImmutableHashMultiSet<T> SetCount(T item, int count)
    {
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
        if (count == 0) return Remove(item);

        if (_counts.TryGetValue(item, out int existing))
        {
            if (existing == count)
            {
                return this;
            }
            else
            {
                return new ImmutableHashMultiSet<T>(_counts.SetItem(item, count), _totalCount + (count - existing));
            }
        }
        else
        {
            return new ImmutableHashMultiSet<T>(_counts.Add(item, count), _totalCount + count);
        }
    }
    IImmutableMultiSet<T> IImmutableMultiSet<T>.SetCount(T item, int count)
    {
        return SetCount(item, count);
    }

    public IReadOnlySet<IReadOnlyCollection<T>> ItemCollections => this;
    public IReadOnlySet<T> UniqueItems => new UniqueItemsView(_counts, _counts.KeyComparer);
    public IReadOnlyDictionary<T, int> ItemCounts => this;

    // IReadOnlyDictionary<T, int>.Keys
    public IEnumerable<T> Keys => _counts.Keys;
    // IReadOnlyDictionary<T, int>.Values
    public IEnumerable<int> Values => _counts.Values;

    // IReadOnlyCollection<T>.Count
    public int Count => _totalCount;
    int IReadOnlyCollection<IReadOnlyCollection<T>>.Count => CountUnique;
    int IReadOnlyCollection<KeyValuePair<T, int>>.Count => CountUnique;

    public int CountUnique => _counts.Count;

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
        return ItemCollections.GetEnumerator();
    }

    // IReadOnlySet<IReadOnlyCollection<T>>.SetEquals(IEnumerable<T1>)
    public bool SetEquals(IEnumerable<IReadOnlyCollection<T>> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        HashSet<KeyValuePair<T, int>> ours =
            new(_counts, new KeyValuePairComparer(_counts.KeyComparer));
        HashSet<KeyValuePair<T, int>> theirs = ToPairSet(other);
        return ours.SetEquals(theirs);
    }

    // IReadOnlySet<IReadOnlyCollection<T>>.IsSubsetOf(IEnumerable<T1>)
    public bool IsSubsetOf(IEnumerable<IReadOnlyCollection<T>> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        HashSet<KeyValuePair<T, int>> ours =
            new(_counts, new KeyValuePairComparer(_counts.KeyComparer));
        HashSet<KeyValuePair<T, int>> theirs = ToPairSet(other);
        return ours.IsSubsetOf(theirs);
    }

    // IReadOnlySet<IReadOnlyCollection<T>>.IsProperSubsetOf(IEnumerable<T1>)
    public bool IsProperSubsetOf(IEnumerable<IReadOnlyCollection<T>> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        HashSet<KeyValuePair<T, int>> ours =
            new(_counts, new KeyValuePairComparer(_counts.KeyComparer));
        HashSet<KeyValuePair<T, int>> theirs = ToPairSet(other);
        return ours.IsProperSubsetOf(theirs);
    }

    // IReadOnlySet<IReadOnlyCollection<T>>.IsSupersetOf(IEnumerable<T1>)
    public bool IsSupersetOf(IEnumerable<IReadOnlyCollection<T>> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        HashSet<KeyValuePair<T, int>> ours =
            new(_counts, new KeyValuePairComparer(_counts.KeyComparer));
        HashSet<KeyValuePair<T, int>> theirs = ToPairSet(other);
        return ours.IsSupersetOf(theirs);
    }

    // IReadOnlySet<IReadOnlyCollection<T>>.IsProperSupersetOf(IEnumerable<T1>)
    public bool IsProperSupersetOf(IEnumerable<IReadOnlyCollection<T>> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        HashSet<KeyValuePair<T, int>> ours =
            new(_counts, new KeyValuePairComparer(_counts.KeyComparer));
        HashSet<KeyValuePair<T, int>> theirs = ToPairSet(other);
        return ours.IsProperSupersetOf(theirs);
    }

    // IReadOnlySet<IReadOnlyCollection<T>>.Overlaps(IEnumerable<T1>)
    public bool Overlaps(IEnumerable<IReadOnlyCollection<T>> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        HashSet<KeyValuePair<T, int>> ours =
            new(_counts, new KeyValuePairComparer(_counts.KeyComparer));
        HashSet<KeyValuePair<T, int>> theirs = ToPairSet(other);
        return ours.Overlaps(theirs);
    }

    private bool ValidateDuplicateGrouping(IReadOnlyCollection<T> items)
    {
        if (items.Count == 0) return false;
        T first = items.First();
        return items.All(item => _counts.KeyComparer.Equals(item, first));
    }

    private HashSet<KeyValuePair<T, int>> ToPairSet(IEnumerable<IReadOnlyCollection<T>> source)
    {
        HashSet<KeyValuePair<T, int>> set = new(new KeyValuePairComparer(_counts.KeyComparer));
        foreach (IReadOnlyCollection<T> collection in source)
        {
            if (ValidateDuplicateGrouping(collection))
            {
                set.Add(new KeyValuePair<T, int>(collection.First(), collection.Count));
            }
            else
            {
                // If the collection isn't a valid grouping of duplicates, add a dummy pair that
                // won't match any valid pair in `_counts` (because `_counts` never contains a pair
                // with a count of -1, even if `default` is potentially valid). This will allow
                // logic to do the right thing whatever the operation will do with non-matching data
                // in the input set.
                set.Add(new KeyValuePair<T, int>(default!, -1));
            }
        }
        return set;
    }

    public Builder ToBuilder() => new(this);

    public sealed class Builder : IMultiSet<T>
    {
        private readonly ImmutableDictionary<T, int>.Builder _counts;
        private int _totalCount;

        internal Builder(ImmutableHashMultiSet<T> set)
        {
            _counts = set._counts.ToBuilder();
            _totalCount = set._totalCount;
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
        public IReadOnlySet<T> UniqueItems => new UniqueItemsView(_counts, _counts.KeyComparer);
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
            T first = items.First();

            return items.All(item => _counts.KeyComparer.Equals(item, first)) &&
                _counts.TryGetValue(first, out int count) &&
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
            return ToImmutable().SetEquals(other);
        }

        // IReadOnlySet<IReadOnlyCollection<T>>.IsSubsetOf(IEnumerable<T1>)
        public bool IsSubsetOf(IEnumerable<IReadOnlyCollection<T>> other)
        {
            return ToImmutable().IsSubsetOf(other);
        }

        // IReadOnlySet<IReadOnlyCollection<T>>.IsProperSubsetOf(IEnumerable<T1>)
        public bool IsProperSubsetOf(IEnumerable<IReadOnlyCollection<T>> other)
        {
            return ToImmutable().IsProperSubsetOf(other);
        }

        // IReadOnlySet<IReadOnlyCollection<T>>.IsSupersetOf(IEnumerable<T1>)
        public bool IsSupersetOf(IEnumerable<IReadOnlyCollection<T>> other)
        {
            return ToImmutable().IsSupersetOf(other);
        }

        // IReadOnlySet<IReadOnlyCollection<T>>.IsProperSupersetOf(IEnumerable<T1>)
        public bool IsProperSupersetOf(IEnumerable<IReadOnlyCollection<T>> other)
        {
            return ToImmutable().IsProperSupersetOf(other);
        }

        // IReadOnlySet<IReadOnlyCollection<T>>.Overlaps(IEnumerable<T1>)
        public bool Overlaps(IEnumerable<IReadOnlyCollection<T>> other)
        {
            return ToImmutable().Overlaps(other);
        }

        public ImmutableHashMultiSet<T> ToImmutable() => new(_counts.ToImmutable(), _totalCount);
    }

    private sealed class KeyValuePairComparer(IEqualityComparer<T> cmp)
        : IEqualityComparer<KeyValuePair<T, int>>
    {
        private readonly IEqualityComparer<T> _cmp = cmp;

        public bool Equals(KeyValuePair<T, int> x, KeyValuePair<T, int> y)
        {
            if (x.Value < 0 || y.Value < 0)
            {
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
                return -1.GetHashCode();
            }
            else
            {
                return HashCode.Combine(_cmp.GetHashCode(obj.Key), obj.Value);
            }
        }
    }

    private sealed class UniqueItemsView(
        IReadOnlyDictionary<T, int> parentCounts, IEqualityComparer<T> comparer)
    : IReadOnlySet<T>
    {
        private readonly IReadOnlyDictionary<T, int> _parentCounts = parentCounts;
        private readonly IEqualityComparer<T> _comparer = comparer;

        public int Count => _parentCounts.Count;
        public bool Contains(T item) => _parentCounts.ContainsKey(item);
        public IEnumerator<T> GetEnumerator() => _parentCounts.Keys.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool IsProperSubsetOf(IEnumerable<T> other) => ToSet().IsProperSubsetOf(other);
        public bool IsProperSupersetOf(IEnumerable<T> other) => ToSet().IsProperSupersetOf(other);
        public bool IsSubsetOf(IEnumerable<T> other) => ToSet().IsSubsetOf(other);
        public bool IsSupersetOf(IEnumerable<T> other) => ToSet().IsSupersetOf(other);
        public bool Overlaps(IEnumerable<T> other) => ToSet().Overlaps(other);
        public bool SetEquals(IEnumerable<T> other) => ToSet().SetEquals(other);

        private HashSet<T> ToSet() => new(_parentCounts.Keys, _comparer);
    }
}
