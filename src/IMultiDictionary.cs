// .Net Utils - Misc utility classes/functions for use in .Net libraries.
// Written in 2026 by Eric Orth
//
// To the extent possible under law, the author(s) have dedicated all copyright and related and
// neighboring rights to this software to the public domain worldwide. This software is distributed
// without any warranty.
//
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If
// not, see http://creativecommons.org/publicdomain/zero/1.0/.

// Interfaces for multi-dictionaries, i.e. dictionaries that can associate multiple values with a
// single key. These interfaces make no assumptions about the ordering or uniqueness of the values
// associated with a given key, typically returning ICollection<TValue> for methods dealing with
// such collections.

using System.Collections;
using System.Collections.Immutable;

namespace DotNetUtils
{
    public interface IMultiDictionary<TKey, TValue>
    : IDictionary<TKey, ICollection<TValue>>, IDictionary,
      IReadOnlyMultiDictionary<TKey, TValue>
    {
        public new IReadOnlyCollection<TKey> Keys { get; }
        public new IReadOnlyCollection<TValue> Values { get; }
        public new IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator();

        public void Add(TKey key, TValue value);
        public void Add(TKey key, IEnumerable<TValue> values);
        public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items);
        public void AddRange(IEnumerable<KeyValuePair<TKey, IEnumerable<TValue>>> items);

        // Removes a single matching value from the collection of values for `key`. Returns true iff
        // a value was removed, false if `key` was not found or did not have a matching value.
        public bool RemoveOne(TKey key, TValue value);

        // Removes a single matching key-value pair (if found). Returns true iff a pair was removed.
        public bool RemoveOne(KeyValuePair<TKey, TValue> item);

        // Removes all matching values from the collection of values for `key`. Returns the number
        // of values successfully removed.
        public int Remove(TKey key, TValue value);

        // Removes all matching key-value pairs. Returns the number of pairs successfully removed.
        public int Remove(KeyValuePair<TKey, TValue> item);

        // Removes all values for `key` iff the collection of values for that key matches (in any
        // order) the provided `values`. Returns the number of values successfully removed.
        public int Remove(TKey key, IEnumerable<TValue> values);

        // Removes all values for `item.Key` iff the collection of values for that key matches (in
        // any order) the collection of values in `item.Value`. Returns the number of values
        // successfully removed.
        public int Remove(KeyValuePair<TKey, IEnumerable<TValue>> item);

        // Removes all values for `key`. Returns the number of values successfully removed.
        public new int Remove(TKey key);

        // Same as `Remove(TKey key)`, but also returns the collection of removed values in
        // `removedValues`.
        public int Remove(TKey key, out ICollection<TValue> removedValues);

        // Removes a single matching key-value pair (if found) for each pair in `items`. Returns the
        // number of key-value pairs successfully removed.
        public int RemoveOneRange(IEnumerable<KeyValuePair<TKey, TValue>> items);

        // Removes all matching key-value pairs for each pair in `items`. Returns the number of
        // key-value pairs successfully removed.
        public int RemoveRange(IEnumerable<KeyValuePair<TKey, TValue>> items);

        // For each key-value pair in `items`, removes all values for `item.Key` iff the collection
        // of values for that key matches (in any order) the collection of values in `items.Value`.
        // Returns the number of values successfully removed.
        public int RemoveRange(IEnumerable<KeyValuePair<TKey, IEnumerable<TValue>>> items);

        // Removes all values for all keys in `keys`. Returns the number of values successfully
        // removed.
        public int RemoveRange(IEnumerable<TKey> keys);
    }

    public interface IReadOnlyMultiDictionary<TKey, TValue>
    : IReadOnlyDictionary<TKey, IReadOnlyCollection<TValue>>,
      IEnumerable<KeyValuePair<TKey, TValue>>
    {
        //!! Here and immutable: Consider separating out a keys collection and a distinct-keys
        //!! collection. Similar with values, although that would be less expected compared to dict.
        public new IReadOnlyCollection<TKey> Keys { get; }
        public new IReadOnlyCollection<TValue> Values { get; }
        public IReadOnlyCollection<IReadOnlyCollection<TValue>> ValueCollections { get; }
        public new IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator();

        // Returns true iff at least one instance of `value` is found in the collection of values
        // for any key.
        public bool ContainsValue(TValue value);
        // Returns true iff `values` matches (in any order) the entire collection of values for at
        // least one key.
        public bool ContainsValue(IEnumerable<TValue> values);
        // Returns true iff at least one instance of `item.Value` is found in the collection of
        // values for `item.Key`.
        public bool Contains(KeyValuePair<TKey, TValue> item);
        // Returns true iff `item.Value` matches (in any order) the entire collection of values for
        // `item.Key`.
        public bool Contains(KeyValuePair<TKey, IEnumerable<TValue>> items);
    }

    public interface IImmutableMultiDictionary<TKey, TValue>
    : IImmutableDictionary<TKey, IImmutableCollection<TValue>>,
      IReadOnlyDictionary<TKey, IImmutableCollection<TValue>>,
      IReadOnlyMultiDictionary<TKey, TValue>
    {
        public new IImmutableCollection<TKey> Keys { get; }
        public new IImmutableCollection<TValue> Values { get; }
        public new IImmutableCollection<IImmutableCollection<TValue>> ValueCollections { get; }
        public new IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator();

        public IImmutableMultiDictionary<TKey, TValue> Add(TKey key, TValue value);
        //!! Confirm this covers the case of adding by IImmutableCollection<TValue> and
        //!! IReadOnlyCollection<TValue> as well. It's weird that I don't get an error here about
        //!! hiding the method in the base interface.
        public IImmutableMultiDictionary<TKey, TValue> Add(TKey key, IEnumerable<TValue> values);
        public IImmutableMultiDictionary<TKey, TValue> AddRange(
            IEnumerable<KeyValuePair<TKey, TValue>> items);
        //!! Confirm this covers the case of adding by IImmutableCollection<TValue> and
        //!! IReadOnlyCollection<TValue> as well.
        public IImmutableMultiDictionary<TKey, TValue> AddRange(
            IEnumerable<KeyValuePair<TKey, IEnumerable<TValue>>> items);
        
        public new IImmutableMultiDictionary<TKey, TValue> Clear();

        // Removes a single matching value from the collection of values for `key`.
        public IImmutableMultiDictionary<TKey, TValue> RemoveOne(TKey key, TValue value);

        // Removes a single matching key-value pair (if found).
        public IImmutableMultiDictionary<TKey, TValue> RemoveOne(KeyValuePair<TKey, TValue> item);

        // Removes all matching values from the collection of values for `key`.
        public IImmutableMultiDictionary<TKey, TValue>  Remove(TKey key, TValue value);

        // Removes all matching key-value pairs.
        public IImmutableMultiDictionary<TKey, TValue>  Remove(KeyValuePair<TKey, TValue> item);

        // Removes all values for `key` iff the collection of values for that key matches (in any
        // order) the provided `values`.
        public IImmutableMultiDictionary<TKey, TValue> Remove(TKey key, IEnumerable<TValue> values);

        // Removes all values for `item.Key` iff the collection of values for that key matches (in
        // any order) the collection of values in `item.Value`.
        public IImmutableMultiDictionary<TKey, TValue> Remove(
            KeyValuePair<TKey, IEnumerable<TValue>> item);

        // Removes all values for `key`.
        public new IImmutableMultiDictionary<TKey, TValue> Remove(TKey key);

        // Removes a single matching key-value pair (if found) for each pair in `items`.
        public IImmutableMultiDictionary<TKey, TValue> RemoveOneRange(
            IEnumerable<KeyValuePair<TKey, TValue>> items);

        // Removes all matching key-value pairs for each pair in `items`.
        public IImmutableMultiDictionary<TKey, TValue> RemoveRange(
            IEnumerable<KeyValuePair<TKey, TValue>> items);

        // For each key-value pair in `items`, removes all values for `item.Key` iff the collection
        // of values for that key matches (in any order) the collection of values in `items.Value`.
        public IImmutableMultiDictionary<TKey, TValue> RemoveRange(
            IEnumerable<KeyValuePair<TKey, IEnumerable<TValue>>> items);

        // Removes all values for all keys in `keys`.
        public new IImmutableMultiDictionary<TKey, TValue> RemoveRange(IEnumerable<TKey> keys);

        // Sets the collection of values for `key` to `values`, potentially replacing an existing
        // collection of values for that key.
        public IImmutableMultiDictionary<TKey, TValue> SetItem(
            TKey key, IEnumerable<TValue> values);

        // Sets the collection of values for `item.Key` to the values of `item.Value`, potentially
        // replacing an existing collection of values for that key.
        public IImmutableMultiDictionary<TKey, TValue> SetItem(
            KeyValuePair<TKey, IEnumerable<TValue>> item);

        // For each key-value pair in `items`, sets the collection of values for the key to the
        // given values, potentially replacing an existing collection of values for that key.
        public IImmutableMultiDictionary<TKey, TValue> SetItems(
            IEnumerable<KeyValuePair<TKey, IEnumerable<TValue>>> items);
    }
}
