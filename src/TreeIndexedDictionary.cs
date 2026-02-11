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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace DotNetUtils
{
    // A self-balancing binary search tree that allows efficient (generally O(lg(n))) access while
    // also keeping track of node indices to allow efficient (also generally O(lg(n))) random access
    // reads by index.
    public class TreeIndexedDictionary<TKey, TValue>(IComparer<TKey> comparer)
        : ISortedIndexedDictionary<TKey, TValue>
    {
        public TreeIndexedDictionary() : this(Comparer<TKey>.Default) { }

        public TreeIndexedDictionary(IEnumerable<KeyValuePair<TKey, TValue>> entries)
            : this(entries, Comparer<TKey>.Default) { }

        public TreeIndexedDictionary(IEnumerable<KeyValuePair<TKey, TValue>> entries,
                                     IComparer<TKey> comparer) : this(comparer)
        {
            if (entries is null)
            {
                throw new ArgumentNullException(nameof(entries));
            }

            // TODO: Add linear-time complete-tree construction when constructing from an ISortedIndexedDictionary.
            if (entries is TreeIndexedDictionary<TKey, TValue> tree && tree.Comparer == comparer)
            {
                if (tree._root is not null)
                {
                    _root = (Node)tree._root.Clone();
                }
            }
            else
            {
                foreach (KeyValuePair<TKey, TValue> entry in entries)
                {
                    Add(entry.Key, entry.Value);
                }
            }
        }

        public object SyncRoot => this;

        public TValue this[TKey key]
        {
            get
            {
                IndexedNode? indexedNode = TryFindNodeByKey(key);
                if (indexedNode.HasValue)
                {
                    return indexedNode.Value.Node.Value;
                }
                else
                {
                    throw new KeyNotFoundException();
                }
            }
            set
            {
                IndexedNode? indexedNode = TryFindNodeByKey(key);
                if (indexedNode.HasValue)
                {
                    indexedNode.Value.Node.Value = value;
                    return;
                }

                Add(key, value);
            }
        }

        public object? this[object key]
        {
            get
            {
                if (key is TKey k && TryGetValue(k, out TValue? value))
                {
                    return value;
                }
                else
                {
                    return null;
                }
            }

            set
            {
                if (key is null)
                {
                    throw new ArgumentNullException(nameof(key));
                }
                else if (key is not TKey)
                {
                    throw new ArgumentException("Must be assignable to " + typeof(TKey).Name,
                                                nameof(key));
                }
                else if (value is not TValue)
                {
                    throw new ArgumentException("Must be assignable to " + typeof(TValue).Name,
                                                nameof(value));
                }
                else
                {
                    this[(TKey)key] = (TValue)value;
                }
            }
        }

        KeyValuePair<TKey, TValue> IReadOnlyList<KeyValuePair<TKey, TValue>>.this[int index]
            => At(index);

        TValue IDictionary<TKey, TValue>.this[TKey key] { get => this[key]; set => this[key] = value; }

        public IEnumerable<TKey> Keys
        {
            get
            {
                IEnumerator<KeyValuePair<TKey, TValue>> enumerator = GetEnumerator();

                while (true)
                {
                    if (enumerator.MoveNext())
                    {
                        yield return enumerator.Current.Key;
                    }
                    else
                    {
                        yield break;
                    }
                }
            }
        }

        ICollection IDictionary.Keys => Keys.ToList();

        public IEnumerable<TValue> Values
        {
            get
            {
                IEnumerator<KeyValuePair<TKey, TValue>> enumerator = GetEnumerator();

                while (true)
                {
                    if (enumerator.MoveNext())
                    {
                        yield return enumerator.Current.Value;
                    }
                    else
                    {
                        yield break;
                    }
                }
            }
        }

        ICollection IDictionary.Values => Values.ToList();

        public int Count
        {
            get
            {
                if (_root is null)
                {
                    return 0;
                }
                else
                {
                    return _root.SubtreeCount;
                }
            }
        }

        public bool IsReadOnly => false;

        public bool IsFixedSize => false;

        public bool IsSynchronized => false;

        ICollection<TKey> IDictionary<TKey, TValue>.Keys =>
            new KeyCollection(this);

        ICollection<TValue> IDictionary<TKey, TValue>.Values =>
            new ValueCollection(this);

        public IComparer<TKey> Comparer { get; } = comparer;

        public void Add(TKey key, TValue value)
        {
            int countBefore = Count;

            if (_root is null)
            {
                _root = new Node(key, value)
                {
                    IsRed = false
                };
                return;
            }

            Node node = _root;
            while (true)
            {
                int comparison = Comparer.Compare(key, node.Key);
                if (comparison == 0)
                {
                    throw new ArgumentException("Key already in dictionary.", nameof(key));
                }
                else if (comparison < 0)
                {
                    if (node.Left is null)
                    {
                        Node toAdd = new(key, value);
                        node.AttachLeft(toAdd);
                        node = toAdd;
                        break;
                    }
                    else
                    {
                        node = node.Left;
                    }
                }
                else
                {
                    Debug.Assert(comparison > 0);
                    if (node.Right is null)
                    {
                        Node toAdd = new(key, value);
                        node.AttachRight(toAdd);
                        node = toAdd;
                        break;
                    }
                    else
                    {
                        node = node.Right;
                    }
                }
            }

            RebalanceAddition(node);

            Debug.Assert(Count == countBefore + 1);
            ValidateTree();
        }

        public void Add(object key, object? value)
        {
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            else if (key is not TKey)
            {
                throw new ArgumentException("Must be assignable to type " + typeof(TKey).Name,
                                            nameof(key));
            }
            else if (value is not TValue)
            {
                throw new ArgumentException("Must be assignable to type " + typeof(TValue).Name,
                                            nameof(value));
            }
            else
            {
                Add((TKey)key, (TValue)value);
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            // TODO: Consider using join operations to make more efficient.
            foreach (KeyValuePair<TKey, TValue> item in items)
            {
                Add(item);
            }
        }

        public void Clear()
        {
            _root = null;
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            IndexedNode? indexedNode = TryFindNodeByKey(item.Key);
            return indexedNode.HasValue && Equals(indexedNode.Value.Node.Value, item.Value);
        }

        public bool ContainsKey(TKey key)
        {
            return TryFindNodeByKey(key).HasValue;
        }

        public bool Contains(object key)
        {
            if (key is TKey k)
            {
                return ContainsKey(k);
            }
            else if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            else
            {
                return false;
            }
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (array is null)
            {
                throw new ArgumentNullException(nameof(array));
            }
            else if (arrayIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            }
            else if (Count > array.Length - arrayIndex)
            {
                throw new ArgumentException("Not enough capacity for copied elements.");
            }

            foreach (KeyValuePair<TKey, TValue> entry in this)
            {
                array[arrayIndex++] = entry;
            }
        }

        public void CopyTo(Array array, int arrayIndex)
        {
            if (array is null)
            {
                throw new ArgumentNullException(nameof(array));
            }
            else if (array.Rank != 1)
            {
                throw new ArgumentException("Should not be multidimensional.", nameof(array));
            }
            else if (arrayIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            }
            else if (Count > array.Length - arrayIndex)
            {
                throw new ArgumentException("Not enough capacity for copied elements.");
            }

            foreach (KeyValuePair<TKey, TValue> entry in this)
            {
                array.SetValue(entry, arrayIndex++);
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            if (_root is null)
            {
                Debug.Assert(Count == 0);
                yield break;
            }

            Node node = _root;
            while (node.Left is not null)
            {
                node = node.Left;
            }
            yield return KeyValuePair.Create(node.Key, node.Value);

            while (true)
            {
                if (node.Parent is null && node != _root)
                {
                    throw new InvalidOperationException("Current node removed during enumeration.");
                }
                else if (node.Right is null)
                {
                    while (true)
                    {
                        if (node.Parent is null)
                        {
                            yield break;
                        }
                        else if (node.IsLeftChild)
                        {
                            node = node.Parent;
                            break;
                        }
                        else
                        {
                            Debug.Assert(node.IsRightChild);
                            node = node.Parent;
                        }
                    }
                }
                else
                {
                    node = node.Right;
                    while (node.Left is not null)
                    {
                        node = node.Left;
                    }
                }

                yield return KeyValuePair.Create(node.Key, node.Value);
            }
        }

        public int IndexOf(TKey key)
        {
            IndexedNode? indexedNode = TryFindNodeByKey(key);
            if (indexedNode.HasValue)
            {
                return indexedNode.Value.Index;
            }
            else
            {
                throw new KeyNotFoundException();
            }
        }

        public int IndexOf(KeyValuePair<TKey, TValue> item)
        {
            IndexedNode? indexedNode = TryFindNodeByKey(item.Key);
            if (indexedNode.HasValue && Equals(indexedNode.Value.Node.Value, item.Value))
            {
                return indexedNode.Value.Index;
            }
            else
            {
                throw new KeyNotFoundException();
            }
        }

        public KeyValuePair<TKey, TValue> At(Index index)
        {
            int offset = index.GetOffset(Count);
            if (offset < 0 || offset >= Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            Node node = FindNodeByOffset(offset);
            return KeyValuePair.Create(node.Key, node.Value);
        }

        public TreeIndexedDictionary<TKey, TValue> At(Range range)
        {
            // TODO: Consider using cloned slicing or linear complete-tree building to make more
            // efficient.

            (int offset, int length) = range.GetOffsetAndLength(Count);
            if (offset < 0 || offset + length > Count)
            {
                throw new ArgumentOutOfRangeException(nameof(range));
            }

            TreeIndexedDictionary<TKey, TValue> slice = new(Comparer);
            for (int i = offset; i < offset + length; i++)
            {
                slice.Add(At(i));
            }
            return slice;
        }

        ISortedIndexedDictionary<TKey, TValue> IReadOnlySortedIndexedDictionary<TKey, TValue>.At(Range range)
        {
            return At(range);
        }

        IIndexedDictionary<TKey, TValue> IReadOnlyIndexedDictionary<TKey, TValue>.At(Range range)
        {
            return At(range);
        }

        public TreeIndexedDictionary<TKey, TValue> Slice(TKey first, TKey last)
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

        ISortedIndexedDictionary<TKey, TValue>
            IReadOnlySortedIndexedDictionary<TKey, TValue>.Slice(TKey first, TKey last)
        {
            return Slice(first, last);
        }

        public void SetValueAt(Index index, TValue value)
        {
            int offset = index.GetOffset(Count);
            if (offset < 0 || offset >= Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            Node node = FindNodeByOffset(offset);
            node.Value = value;
        }

        public bool Remove(TKey key)
        {
            IndexedNode? indexedNode = TryFindNodeByKey(key);
            if (!indexedNode.HasValue)
            {
                return false;
            }

            RemoveNode(indexedNode.Value);
            return true;
        }

        public void Remove(object key)
        {
            if (key is TKey k)
            {
                Remove(k);
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            IndexedNode? indexedNode = TryFindNodeByKey(item.Key);
            if (!indexedNode.HasValue || !Equals(indexedNode.Value.Node.Value, item.Value))
            {
                return false;
            }

            RemoveNode(indexedNode.Value);
            return true;
        }

        public void RemoveRange(IEnumerable<TKey> keys)
        {
            if (keys is null)
            {
                throw new ArgumentNullException(nameof(keys));
            }

            foreach(TKey key in keys)
            {
                Remove(key);
            }
        }

        public void RemoveAt(Index index)
        {
            int offset = index.GetOffset(Count);
            if (offset < 0 || offset >= Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            Node node = FindNodeByOffset(offset);
            RemoveNode(new IndexedNode(node, offset));
        }

        public void RemoveAt(Range range)
        {
            // TODO: Consider implementing split/join operations to make this more efficient.

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

            RemoveRange(toRemove);
        }

        public KeyValuePair<TKey, TValue>? TryGetLowerItem(TKey key)
        {
            IndexedNode? indexedNode = TryGetLowerNode(key);
            if (indexedNode.HasValue)
            {
                return KeyValuePair.Create(indexedNode.Value.Node.Key,
                                           indexedNode.Value.Node.Value);
            }
            else
            {
                return null;
            }
        }

        public int? TryGetIndexOfLowerItem(TKey key)
        {
            IndexedNode? indexedNode = TryGetLowerNode(key);
            if (indexedNode.HasValue)
            {
                return indexedNode.Value.Index;
            }
            else
            {
                return null;
            }
        }

        public KeyValuePair<TKey, TValue>? TryGetFloorItem(TKey key)
        {
            IndexedNode? indexedNode = TryGetFloorNode(key);
            if (indexedNode.HasValue)
            {
                return KeyValuePair.Create(indexedNode.Value.Node.Key,
                                           indexedNode.Value.Node.Value);
            }
            else
            {
                return null;
            }
        }

        public int? TryGetIndexOfFloorItem(TKey key)
        {
            IndexedNode? indexedNode = TryGetFloorNode(key);
            if (indexedNode.HasValue)
            {
                return indexedNode.Value.Index;
            }
            else
            {
                return null;
            }
        }

        public KeyValuePair<TKey, TValue>? TryGetCeilingItem(TKey key)
        {
            IndexedNode? indexedNode = TryGetCeilingNode(key);
            if (indexedNode.HasValue)
            {
                return KeyValuePair.Create(indexedNode.Value.Node.Key,
                                           indexedNode.Value.Node.Value);
            }
            else
            {
                return null;
            }
        }

        public int? TryGetIndexOfCeilingItem(TKey key)
        {
            IndexedNode? indexedNode = TryGetCeilingNode(key);
            if (indexedNode.HasValue)
            {
                return indexedNode.Value.Index;
            }
            else
            {
                return null;
            }
        }

        public KeyValuePair<TKey, TValue>? TryGetHigherItem(TKey key)
        {
            IndexedNode? indexedNode = TryGetHigherNode(key);
            if (indexedNode.HasValue)
            {
                return KeyValuePair.Create(indexedNode.Value.Node.Key,
                                           indexedNode.Value.Node.Value);
            }
            else
            {
                return null;
            }
        }

        public int? TryGetIndexOfHigherItem(TKey key)
        {
            IndexedNode? indexedNode = TryGetHigherNode(key);
            if (indexedNode.HasValue)
            {
                return indexedNode.Value.Index;
            }
            else
            {
                return null;
            }
        }

        public bool TryGetKey(TKey key, out TKey actual)
        {
            IndexedNode? indexedNode = TryFindNodeByKey(key);
            if (indexedNode.HasValue)
            {
                actual = indexedNode.Value.Node.Key;
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
            IndexedNode? indexedNode = TryFindNodeByKey(key);
            if (indexedNode.HasValue)
            {
                value = indexedNode.Value.Node.Value;
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return new DictionaryEnumerator(GetEnumerator());
        }

        private void ValidateTree()
        {
            if (_root is null)
            {
                Debug.Assert(Count == 0);
                return;
            }
            else if (_root.IsRed)
            {
                throw new InvalidOperationException("Root is red.");
            }
            else
            {
                int? totalBlackHeight = null;
                int count = ValidateTree(_root, 0, ref totalBlackHeight);
                Debug.Assert(count == Count);
            }
        }

        private static int ValidateTree(Node node, int blackHeight,
                                        ref int? totalBlackHeight)
        {
            Debug.Assert(node is not null);

            if (node.IsRed)
            {
                if (node.Parent is not null && node.Parent.IsRed)
                {
                    throw new InvalidOperationException("Double-red violation.");
                }
            }
            else
            {
                blackHeight++;
            }

            if (node.Left is null || node.Right is null)
            {
                if (totalBlackHeight is null)
                {
                    totalBlackHeight = blackHeight;
                }
                else if (blackHeight != totalBlackHeight)
                {
                    throw new InvalidOperationException(
                        "Black height violation: " + blackHeight + " vs " + totalBlackHeight + ".");
                }
            }

            int count = 1;
            if (node.Left is null)
            {
                Debug.Assert(node.NumLeft == 0);
            }
            else
            {
                Debug.Assert(node.Left.Parent == node);
                int numLeft = ValidateTree(node.Left, blackHeight,
                                           ref totalBlackHeight);
                Debug.Assert(node.NumLeft == numLeft);
                count += numLeft;
            }
            if (node.Right is null)
            {
                Debug.Assert(node.NumRight == 0);
            }
            else
            {
                Debug.Assert(node.Right.Parent == node);
                int numRight = ValidateTree(node.Right, blackHeight,
                                            ref totalBlackHeight);
                Debug.Assert(node.NumRight == numRight);
                count += numRight;
            }

            Debug.Assert(count == node.SubtreeCount);

            return count;
        }

        private class KeyCollection(TreeIndexedDictionary<TKey, TValue> tree)
            : ICollection<TKey>, IReadOnlyCollection<TKey>
        {
            public int Count => _tree.Count;

            public bool IsReadOnly => true;

            public void Add(TKey item)
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            public bool Contains(TKey item)
            {
                return _tree.TryFindNodeByKey(item).HasValue;
            }

            public void CopyTo(TKey[] array, int arrayIndex)
            {
                if (array is null)
                {
                    throw new ArgumentNullException(nameof(array));
                }
                else if (Count > array.Length - arrayIndex)
                {
                    throw new ArgumentException("Not enough capacity for copied elements.");
                }

                foreach (TKey key in this)
                {
                    array[arrayIndex++] = key;
                }
            }

            public IEnumerator<TKey> GetEnumerator()
            {
                return _tree.Keys.GetEnumerator();
            }

            public bool Remove(TKey item)
            {
                throw new NotSupportedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            private readonly TreeIndexedDictionary<TKey, TValue> _tree = tree;
        }

        private class ValueCollection(TreeIndexedDictionary<TKey, TValue> tree)
            : ICollection<TValue>, IReadOnlyCollection<TValue>
        {
            public int Count => _tree.Count;

            public bool IsReadOnly => true;

            public void Add(TValue item)
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            public bool Contains(TValue item)
            {
                foreach (TValue value in this)
                {
                    if (Equals(value, item))
                    {
                        return true;
                    }
                }

                return false;
            }

            public void CopyTo(TValue[] array, int arrayIndex)
            {
                if (array is null)
                {
                    throw new ArgumentNullException(nameof(array));
                }
                else if (Count > array.Length - arrayIndex)
                {
                    throw new ArgumentException("Not enough capacity for copied elements.");
                }

                foreach (TValue value in this)
                {
                    array[arrayIndex++] = value;
                }
            }

            public IEnumerator<TValue> GetEnumerator()
            {
                return _tree.Values.GetEnumerator();
            }

            public bool Remove(TValue item)
            {
                throw new NotSupportedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            private readonly TreeIndexedDictionary<TKey, TValue> _tree = tree;
        }

        private class DictionaryEnumerator(IEnumerator<KeyValuePair<TKey, TValue>> subEnumerator)
            : IDictionaryEnumerator
        {
            public DictionaryEntry Entry => new(Key, Value);

            public object Key
            {
                get
                {
                    Debug.Assert(_enumerator.Current.Key is not null);
                    return _enumerator.Current.Key;
                }
            }

            public object? Value => _enumerator.Current.Value;

            public object Current => Entry;

            public bool MoveNext()
            {
                return _enumerator.MoveNext();
            }

            public void Reset()
            {
                _enumerator.Reset();
            }

            private readonly IEnumerator<KeyValuePair<TKey, TValue>> _enumerator = subEnumerator;
        }

        private readonly struct IndexedNode(Node node, int index)
        {
            public Node Node { get; } = node;
            public int Index { get; } = index;
        }

        private IndexedNode? TryFindNodeByKey(TKey key)
        {
            Node? node = _root;
            int numLeft = 0;
            while (true)
            {
                if (node is null)
                {
                    return null;
                }

                int comparison = Comparer.Compare(key, node.Key);
                if (comparison == 0)
                {
                    return new IndexedNode(node, numLeft + node.NumLeft);
                }
                else if (comparison < 0)
                {
                    node = node.Left;
                }
                else
                {
                    Debug.Assert(comparison > 0);
                    numLeft += node.NumLeft + 1;
                    node = node.Right;
                }
            }
        }

        private IndexedNode? TryGetCeilingNode(TKey key)
        {
            if (_root is null)
            {
                return null;
            }

            Node node = _root;
            int numLeft = 0;
            while (true)
            {
                int comparison = Comparer.Compare(key, node.Key);
                if (comparison == 0)
                {
                    return new IndexedNode(node, numLeft + node.NumLeft);
                }
                else if (comparison < 0)
                {
                    if (node.Left is null)
                    {
                        Debug.Assert(node.NumLeft == 0);
                        return new IndexedNode(node, numLeft);
                    }
                    else
                    {
                        node = node.Left;
                    }
                }
                else
                {
                    Debug.Assert(comparison > 0);
                    if (node.Right is null)
                    {
                        if (numLeft + node.NumLeft + 1 >= Count)
                        {
                            return null;
                        }
                        else
                        {
                            Node nextHigher = FindNodeByOffset(numLeft + node.NumLeft + 1);
                            Debug.Assert(Comparer.Compare(nextHigher.Key, key) > 0);
                            return new IndexedNode(nextHigher, numLeft + node.NumLeft + 1);
                        }
                    }
                    else
                    {
                        numLeft += node.NumLeft + 1;
                        node = node.Right;
                    }
                }
            }
        }

        private IndexedNode? TryGetHigherNode(TKey key)
        {
            IndexedNode? ceiling = TryGetCeilingNode(key);
            if (!ceiling.HasValue)
            {
                return null;
            }
            else if (Comparer.Compare(ceiling.Value.Node.Key, key) == 0)
            {
                if (ceiling.Value.Index + 1 < Count)
                {
                    Node nextHigher = FindNodeByOffset(ceiling.Value.Index + 1);
                    return new IndexedNode(nextHigher, ceiling.Value.Index + 1);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                Debug.Assert(Comparer.Compare(ceiling.Value.Node.Key, key) > 0);
                return ceiling;
            }
        }

        private IndexedNode? TryGetFloorNode(TKey key)
        {
            IndexedNode? ceiling = TryGetCeilingNode(key);

            if (ceiling.HasValue)
            {
                Debug.Assert(Comparer.Compare(ceiling.Value.Node.Key, key) >= 0);
                if (Comparer.Compare(ceiling.Value.Node.Key, key) == 0)
                {
                    return ceiling;
                }
                else if (ceiling.Value.Index > 0)
                {

                    Node nextLower = FindNodeByOffset(ceiling.Value.Index - 1);
                    Debug.Assert(Comparer.Compare(nextLower.Key, key) < 0);
                    return new IndexedNode(nextLower, ceiling.Value.Index - 1);
                }
                else
                {
                    return null;
                }
            }
            else if (_root is null)
            {
                return null;
            }
            else
            {
                Node highest = FindNodeByOffset(Count - 1);
                Debug.Assert(Comparer.Compare(highest.Key, key) < 0);
                return new IndexedNode(highest, Count - 1);
            }
        }

        private IndexedNode? TryGetLowerNode(TKey key)
        {
            IndexedNode? floor = TryGetFloorNode(key);
            if (!floor.HasValue)
            {
                return null;
            }
            else if (Comparer.Compare(floor.Value.Node.Key, key) == 0)
            {
                if (floor.Value.Index > 0)
                {
                    Node nextLower = FindNodeByOffset(floor.Value.Index - 1);
                    Debug.Assert(Comparer.Compare(nextLower.Key, key) < 0);
                    return new IndexedNode(nextLower, floor.Value.Index - 1);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                Debug.Assert(Comparer.Compare(floor.Value.Node.Key, key) < 0);
                return floor;
            }
        }

        private Node FindNodeByOffset(int offset)
        {
            Debug.Assert(offset >= 0 && offset < Count);
            Debug.Assert(_root is not null);

            Node node = _root;
            int numLeft = 0;
            while (true)
            {
                if (offset == numLeft + node.NumLeft)
                {
                    return node;
                }
                else if (offset < numLeft + node.NumLeft)
                {
                    Debug.Assert(node.Left is not null);
                    node = node.Left;
                }
                else
                {
                    Debug.Assert(offset > numLeft + node.NumLeft);
                    numLeft += 1 + node.NumLeft;
                    Debug.Assert(node.Right is not null);
                    node = node.Right;
                }
            }
        }

        private void RemoveNode(IndexedNode indexedNode)
        {
            int countBefore = Count;

            if (indexedNode.Node.Left is null && indexedNode.Node.Right is null)
            {
                if (indexedNode.Node.Parent is null)
                {
                    Debug.Assert(Count == 1);
                    _root = null;
                }
                else
                {
                    if (!indexedNode.Node.IsRed)
                    {
                        RebalanceDeletion(indexedNode.Node);
                    }

                    indexedNode.Node.Detach();
                }
            }
            else if (indexedNode.Node.Left is null || indexedNode.Node.Right is null)
            {
                Debug.Assert(!indexedNode.Node.IsRed);

                Node? orphan = indexedNode.Node.Left ?? indexedNode.Node.Right;
                Debug.Assert(orphan is not null);
                Debug.Assert(orphan.IsRed);

                orphan.Detach();
                orphan.IsRed = false;

                Node? parent = indexedNode.Node.Parent;
                if (parent is null)
                {
                    _root = orphan;
                }
                else if (indexedNode.Node.IsLeftChild)
                {
                    indexedNode.Node.Detach();
                    parent.AttachLeft(orphan);
                }
                else
                {
                    Debug.Assert(indexedNode.Node.IsRightChild);
                    indexedNode.Node.Detach();
                    parent.AttachRight(orphan);
                }
            }
            else
            {
                int successorIndex = indexedNode.Index + 1;
                Debug.Assert(successorIndex < Count);

                Node successor = FindNodeByOffset(successorIndex);
                Debug.Assert(successor.Left is null);
                SwapNodes(indexedNode.Node, successor);
                Debug.Assert(indexedNode.Node.Left is null);
                RemoveNode(new IndexedNode(indexedNode.Node, successorIndex));
            }

            Debug.Assert(Count == countBefore - 1);

            indexedNode.Node.Parent = null;
            indexedNode.Node.Left = null;
            indexedNode.Node.Right = null;

            ValidateTree();
        }

        private void SwapNodes(Node n1, Node n2)
        {
            Debug.Assert(n1 != n2);

            Node? parent = n1.Parent;
            Node? left = n1.Left;
            Node? right = n1.Right;
            int subtreeCount = n1.SubtreeCount;
            bool isRed = n1.IsRed;

            n1.Parent = n2.Parent;
            if (n2.Parent is null)
            {
                Debug.Assert(_root == n2);
                _root = n1;
            }
            else if (n2.Parent.Left == n2)
            {
                n2.Parent.Left = n1;
            }
            else
            {
                Debug.Assert(n2.Parent.Right == n2);
                n2.Parent.Right = n1;
            }
            n1.Left = n2.Left;
            if (n1.Left is not null && n1.Left != n1)
            {
                n1.Left.Parent = n1;
            }
            n1.Right = n2.Right;
            if (n1.Right is not null && n1.Right != n1)
            {
                n1.Right.Parent = n1;
            }
            n1.SubtreeCount = n2.SubtreeCount;
            n1.IsRed = n2.IsRed;

            n2.Parent = parent;
            if (parent is null)
            {
                Debug.Assert(_root == n1);
                _root = n2;
            }
            else if (parent.Left == n1)
            {
                parent.Left = n2;
            }
            else
            {
                Debug.Assert(parent.Right == n1);
                parent.Right = n2;
            }
            n2.Left = left;
            if (n2.Left is not null && n2.Left != n2)
            {
                n2.Left.Parent = n2;
            }
            n2.Right = right;
            if (n2.Right is not null && n2.Right != n2)
            {
                n2.Right.Parent = n2;
            }
            n2.SubtreeCount = subtreeCount;
            n2.IsRed = isRed;

            if (n1.Parent is not null && n1.Parent == n1)
            {
                n1.Parent = n2;
            }
            if (n1.Left is not null && n1.Left == n1)
            {
                n1.Left = n2;
            }
            if (n1.Right is not null && n1.Right == n1)
            {
                n1.Right = n2;
            }
            if (n2.Parent is not null && n2.Parent == n2)
            {
                n2.Parent = n1;
            }
            if (n2.Left is not null && n2.Left == n2)
            {
                n2.Left = n1;
            }
            if (n2.Right is not null && n2.Right == n2)
            {
                n2.Right = n1;
            }
        }

        private void RebalanceAddition(Node node)
        {
            while (true)
            {
                Debug.Assert(node.IsRed);

                if (node.Parent is null)
                {
                    node.IsRed = false;
                    return;
                }
                if (!node.Parent.IsRed)
                {
                    return;
                }
                else if (node.Parent.Parent is null)
                {
                    node.Parent.IsRed = false;
                    return;
                }
                Debug.Assert(!node.Parent.Parent.IsRed);

                Node? uncle = node.Uncle;
                if (uncle is not null && uncle.IsRed)
                {
                    node.Parent.IsRed = false;
                    uncle.IsRed = false;
                    node.Parent.Parent.IsRed = true;
                    node = node.Parent.Parent;
                }
                else if (node.IsLeftChild && node.Parent.IsRightChild ||
                         node.IsRightChild && node.Parent.IsLeftChild)
                {
                    Node oldParent = node.Parent;
                    node.RotateWithParent();
                    Debug.Assert(node.Parent is not null);
                    Debug.Assert(oldParent.Parent == node);
                    node = oldParent;
                }
                else
                {
                    Debug.Assert(node.IsLeftChild && node.Parent.IsLeftChild ||
                                 node.IsRightChild && node.Parent.IsRightChild);
                    Node oldGrandparent = node.Parent.Parent;
                    node.Parent.RotateWithParent();
                    Debug.Assert(oldGrandparent.Parent == node.Parent);
                    if (node.Parent.Parent is null)
                    {
                        _root = node.Parent;
                    }
                    node.Parent.IsRed = false;
                    oldGrandparent.IsRed = true;
                    return;
                }
            }
        }

        private void RebalanceDeletion(Node node)
        {
            while (true)
            {
                Debug.Assert(!node.IsRed);

                Node? parent = node.Parent;
                if (parent is null)
                {
                    return;
                }

                Node? sibling = node.Sibling;
                Debug.Assert(sibling is not null);
                Debug.Assert(sibling.Parent is not null);

                (Node? closeNibling, Node? farNibling) = node.Niblings;

                if (!parent.IsRed && !sibling.IsRed &&
                    (closeNibling is null || !closeNibling.IsRed) &&
                    (farNibling is null || !farNibling.IsRed))
                {
                    sibling.IsRed = true;
                    node = parent;
                }
                else if (sibling.IsRed)
                {
                    Debug.Assert(!parent.IsRed);
                    Debug.Assert(closeNibling is null || !closeNibling.IsRed);
                    Debug.Assert(farNibling is null || !farNibling.IsRed);
                    sibling.RotateWithParent();
                    if (sibling.Parent is null)
                    {
                        _root = sibling;
                    }
                    sibling.IsRed = false;
                    parent.IsRed = true;
                }
                else if (parent.IsRed &&
                         (closeNibling is null || !closeNibling.IsRed) &&
                         (farNibling is null || !farNibling.IsRed))
                {
                    Debug.Assert(!sibling.IsRed);
                    parent.IsRed = false;
                    sibling.IsRed = true;
                    return;
                }
                else if (!sibling.IsRed && closeNibling is not null && closeNibling.IsRed &&
                         (farNibling is null || !farNibling.IsRed))
                {
                    closeNibling.RotateWithParent();
                    Debug.Assert(closeNibling.Parent == parent);
                    closeNibling.IsRed = false;
                    sibling.IsRed = true;
                }
                else
                {
                    Debug.Assert(!sibling.IsRed);
                    Debug.Assert(farNibling is not null && farNibling.IsRed);
                    sibling.RotateWithParent();
                    sibling.IsRed = parent.IsRed;
                    if (sibling.Parent is null)
                    {
                        _root = sibling;
                        Debug.Assert(!sibling.IsRed);
                    }
                    parent.IsRed = false;
                    farNibling.IsRed = false;
                    return;
                }
            }
        }

        private class Node(TKey key, TValue value) : ICloneable
        {
            public object Clone()
            {
                var clone = (Node)MemberwiseClone();
                if (clone.Left is not null)
                {
                    clone.Left = (Node)clone.Left.Clone();
                    clone.Left.Parent = clone;
                }
                if (clone.Right is not null)
                {
                    clone.Right = (Node)clone.Right.Clone();
                    clone.Right.Parent = clone;
                }
                return clone;
            }

            public void Detach()
            {
                if (IsLeftChild)
                {
                    Debug.Assert(Parent is not null);
                    Parent.Left = null;
                }
                else if (IsRightChild)
                {
                    Debug.Assert(Parent is not null);
                    Parent.Right = null;
                }

                Node? n = Parent;
                while (n is not null)
                {
                    n.SubtreeCount -= SubtreeCount;
                    Debug.Assert(n.SubtreeCount == 1 + n.NumLeft + n.NumRight);
                    n = n.Parent;
                }

                Parent = null;
            }

            public void AttachLeft(Node node)
            {
                Debug.Assert(Left is null);
                Debug.Assert(node.Parent is null);

                Left = node;
                node.Parent = this;

                Node? n = this;
                while (n is not null)
                {
                    n.SubtreeCount += node.SubtreeCount;
                    Debug.Assert(n.SubtreeCount == 1 + n.NumLeft + n.NumRight);
                    n = n.Parent;
                }
            }

            public void AttachRight(Node node)
            {
                Debug.Assert(Right is null);
                Debug.Assert(node.Parent is null);

                Right = node;
                node.Parent = this;

                Node? n = this;
                while (n is not null)
                {
                    n.SubtreeCount += node.SubtreeCount;
                    Debug.Assert(n.SubtreeCount == 1 + n.NumLeft + n.NumRight);
                    n = n.Parent;
                }
            }

            public void RotateWithParent()
            {
                Debug.Assert(Parent is not null);
                Node parent = Parent;
                Node? grandparent = parent.Parent;

                if (IsLeftChild)
                {
                    Right?.Parent = parent;
                    SubtreeCount += 1 + parent.NumRight;
                    parent.SubtreeCount -= 1 + NumLeft;
                    parent.Left = Right;
                    Right = parent;
                }
                else
                {
                    Debug.Assert(IsRightChild);
                    Left?.Parent = parent;
                    SubtreeCount += 1 + parent.NumLeft;
                    parent.SubtreeCount -= 1 + NumRight;
                    parent.Right = Left;
                    Left = parent;

                }
                Parent = grandparent;

                if (parent.IsLeftChild)
                {
                    Debug.Assert(grandparent is not null);
                    grandparent.Left = this;
                }
                else if (parent.IsRightChild)
                {
                    Debug.Assert(grandparent is not null);
                    grandparent.Right = this;
                }
                parent.Parent = this;
            }

            public bool IsLeftChild
            {
                get
                {
                    if (Parent is null)
                    {
                        return false;
                    }
                    else
                    {
                        return Parent.Left == this;
                    }
                }
            }
            public bool IsRightChild
            {
                get
                {
                    if (Parent is null)
                    {
                        return false;
                    }
                    else
                    {
                        return Parent.Right == this;
                    }
                }
            }

            public Node? Uncle
            {
                get
                {
                    Debug.Assert(Parent is not null && Parent.Parent is not null);
                    return Parent.Sibling;
                }
            }

            public Node? Sibling
            {
                get
                {
                    Debug.Assert(Parent is not null);
                    if (IsLeftChild)
                    {
                        return Parent.Right;
                    }
                    else
                    {
                        Debug.Assert(IsRightChild);
                        return Parent.Left;
                    }
                }
            }

            public (Node? close, Node? far) Niblings
            {
                get
                {
                    Debug.Assert(Parent is not null);
                    Debug.Assert(Sibling is not null);

                    if (Sibling.IsLeftChild)
                    {
                        return (Sibling.Right, Sibling.Left);
                    }
                    else
                    {
                        Debug.Assert(Sibling.IsRightChild);
                        return (Sibling.Left, Sibling.Right);
                    }
                }
            }

            public Node? Parent { get; set; } = null;
            public Node? Left { get; set; } = null;
            public Node? Right { get; set; } = null;

            public TKey Key { get; } = key;
            public TValue Value { get; set; } = value;

            public int NumLeft
            {
                get
                {
                    if (Left is null)
                    {
                        return 0;
                    }
                    else
                    {
                        return Left.SubtreeCount;
                    }
                }
            }
            public int NumRight
            {
                get
                {
                    if (Right is null)
                    {
                        return 0;
                    }
                    else
                    {
                        return Right.SubtreeCount;
                    }
                }
            }
            public int SubtreeCount { get; set; } = 1;

            public bool IsRed { get; set; } = true;
        }

        private Node? _root = null;
    }
}
