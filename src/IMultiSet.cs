// .Net Utils - Misc utility classes/functions for use in .Net libraries.
// Written in 2026 by Eric Orth
//
// To the extent possible under law, the author(s) have dedicated all copyright and related and
// neighboring rights to this software to the public domain worldwide. This software is distributed
// without any warranty.
//
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If
// not, see http://creativecommons.org/publicdomain/zero/1.0/.

// Interfaces for multi-sets, i.e. sets that can contain non-unique values. Abstractly, because they
// can contain duplicates, a multi-set is not actually a set at all but actually closer to a basic
// collection. The primary difference between a multi-set and a basic collection is that the
// duplicate values in a multi-set are grouped together, and thus the IMultiSet<T> interface
// provides methods for adding, removing, and querying groups of duplicate values.

using System.Collections.Immutable;

namespace DotNetUtils
{
    public interface IMultiSet<T> : ICollection<T>, IReadOnlyMultiSet<T>
    {
        // Add `count` instances of `item` to the multi-set.
        public void AddN(T item, int count);

        // Add a single instance of each item in `items` (potentially containing duplicates) to the
        // multi-set.
        public void AddRange(IEnumerable<T> items);

        // Removes a single matching value. Returns true iff a value was successfully removed.
        public bool RemoveOne(T item);

        // Removes up to `count` matching values. Returns the number of values successfully removed.
        public int RemoveN(T item, int count);

        // Removes all matching values. Returns the number of values successfully removed.
        public new int Remove(T item);

        // Removes a single matching value for each item in `items`. Returns the number of values
        // successfully removed.
        public int RemoveOneRange(IEnumerable<T> items);

        // Removes all matching values for each item in `items`. Returns the number of values
        // successfully removed.
        public int RemoveRange(IEnumerable<T> items);

        public void SetCount(T item, int count);
    }

    public interface IReadOnlyMultiSet<T>
    : IReadOnlyCollection<T>, IReadOnlyDictionary<T, int>, IReadOnlySet<IReadOnlyCollection<T>>,
      IEnumerable<T>
    {
        public IReadOnlySet<IReadOnlyCollection<T>> ItemCollections { get; }
        public IReadOnlySet<T> UniqueItems { get; }
        public IReadOnlyDictionary<T, int> ItemCounts { get; }

        // Duplicate values are guaranteed to be grouped together in the enumeration order.
        public new IEnumerator<T> GetEnumerator();

        public int CountUnique { get;}

        // Returns true iff at least one instance of `item` is found in the multi-set.
        public bool Contains(T item);
        // Returns true iff exactly `count` instances of `item` are found in the multi-set.
        public bool Contains(T item, int count);

        public int CountOf(T item);
    }

    public interface IImmutableMultiSet<T>
    : IImmutableCollection<T>, IReadOnlyMultiSet<T>, IEnumerable<T>
    {
        public new IImmutableSet<IImmutableCollection<T>> ItemCollections { get; }
        public new IImmutableSet<T> UniqueItems { get; }
        public new IImmutableDictionary<T, int> ItemCounts { get; }

        public new IImmutableMultiSet<T> Add(T item);

        // Add `count` instances of `item` to the multi-set.
        public IImmutableMultiSet<T> AddN(T item, int count);

        // Add a single instance of each item in `items` (potentially containing duplicates) to the
        // multi-set.
        public IImmutableMultiSet<T> AddRange(IEnumerable<T> items);

        // Removes a single matching value.
        public IImmutableMultiSet<T> RemoveOne(T item);

        // Removes up to `count` matching values.
        public IImmutableMultiSet<T> RemoveN(T item, int count);

        // Removes all matching values.
        public new IImmutableMultiSet<T> Remove(T item);

        // Removes a single matching value for each item in `items`.
        public IImmutableMultiSet<T> RemoveOneRange(IEnumerable<T> items);

        // Removes all matching values for each item in `items`.
        public IImmutableMultiSet<T> RemoveRange(IEnumerable<T> items);

        public new IImmutableMultiSet<T> Clear();

        public IImmutableMultiSet<T> SetCount(T item, int count);
    }
}
