// .Net Utils - Misc utility classes/functions for use in .Net libraries.
// Written in 2023 by Eric Orth
//
// To the extent possible under law, the author(s) have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see http://creativecommons.org/publicdomain/zero/1.0/.

using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;

namespace DotNetUtils
{
    // Utilities to provide immutable list/span slices of given enumerables. Directly wraps or
    // reuses the input enumerable instead of copying from it whenever possible, and using minimal
    // overhead over normal .Take().To*() Linq calls when not.
    //
    // Caveat: When the input enumerable is wrapped or reused, that will prevent it from being
    // garbage collected. Thus these utilities are best for use when that input enumerable is long-
    // lived. When the input is no longer needed after a slice is extracted, it may be better to
    // use normal Linq calls to force a copy of the slice.
    public static class ImmutableListSlice
    {
        // Get an IImmutableList (that will always also implement IList<T>) for the given
        // `enumerable` and `range`. If the input enumerable meets the immutability and type
        // requirements (implements both IList<T> and IImmutableList<T>), takes  advantage of the
        // immutable nature of that input by reusing or wrapping without copy.
        //
        // TODO: Figure out the Linq Take() output to see if this can be reworked to work after a
        // normal Take() call as a more direct mirror of the Linq ToImmutableList().
        public static IImmutableList<T> ToImmutableList<T>(this IEnumerable<T> enumerable, Range range)
        {
            if (enumerable is IList<T> and IImmutableList<T> immutableList)
            {
                int startOffset = range.Start.GetOffset(immutableList.Count);
                int endOffset = range.End.GetOffset(immutableList.Count);
                if (startOffset < 0 || endOffset > immutableList.Count)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(range), "Cannot take slice larger than input enumerable.");
                }
                else if (startOffset >= endOffset)
                {
                    return ImmutableList<T>.Empty;
                }
                else if (startOffset == 0 && endOffset == immutableList.Count)
                {
                    return immutableList;
                }
                else if (enumerable is ImmutableListSlice<T> slice)
                {
                    return new ImmutableListSlice<T>(slice.Inner,
                        (range.Start.GetOffset(slice.Count) + slice.Offset)..(slice.Offset + range.End.GetOffset(slice.Count)));
                }
                else
                {
                    return new ImmutableListSlice<T>(immutableList, range);
                }
            }
            else
            {
                return enumerable.Take(range).ToImmutableList();
            }
        }

        // Get an immutable ReadOnlySpan for the given `enumerable` and `range`, including copying
        // the input enumerable if not an ImmutableArray<T>.
        public static ReadOnlySpan<T> ToImmutableSpan<T>(this IEnumerable<T> enumerable, Range range)
        {
            if (enumerable is ImmutableArray<T> array)
            {
                return array.AsSpan()[range];
            }
            else
            {
                return enumerable.Take(range).ToImmutableArray().AsSpan();
            }
        }
    }

    // Helper to wrap an IImmutableList<T> and provide a slice.
    internal sealed class ImmutableListSlice<T> : IList<T>, IImmutableList<T>
    {
        internal ImmutableListSlice(IImmutableList<T> inner, Range range)
        {
            Debug.Assert(inner is IList<T>);

            (int offset, int count) = range.GetOffsetAndLength(inner.Count);
            Debug.Assert(count >= 0);
            Debug.Assert(offset >= 0 && offset <= inner.Count);
            Debug.Assert(offset + count <= inner.Count);

            _inner = inner;
            _offset = offset;
            _count = count;
        }

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= _count) throw new ArgumentOutOfRangeException(nameof(index));
                return _inner[_offset + index];
            }
            set => throw new NotSupportedException();
        }

        public int Count => _count;

        public bool IsReadOnly => true;

        public void Add(T item)
        {
            throw new NotSupportedException();
        }

        public IImmutableList<T> AddRange(IEnumerable<T> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            int count = items.Count();
            if (count == 0)
            {
                return this;
            }

            ImmutableList<T>.Builder builder = ImmutableList.CreateBuilder<T>();
            for (int i = 0; i < _count; i++)
            {
                builder.Add(_inner[_offset + i]);
            }
            builder.AddRange(items);
            return builder.ToImmutable();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(T item)
        {
            return _inner.IndexOf(item, _offset, _count, null) != -1;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (array.Length < arrayIndex + _count) throw new ArgumentException("Not enough space in destination.");

            for (int i = 0; i < _count; i++)
            {
                array[arrayIndex + i] = _inner[_offset + i];
            }
        }

        private struct Enumerator(ImmutableListSlice<T> list) : IEnumerator<T>
        {
            public readonly T Current
            {
                get
                {
                    if (_index < 0 || _index >= _list.Count)
                    {
                        throw new InvalidOperationException();
                    }
                    else
                    {
                        return _list[_index];
                    }
                }
            }

            readonly object? IEnumerator.Current => Current;

            public bool MoveNext()
            {
                if (_index >= _list.Count)
                {
                    return false;
                }
                else
                {
                    return ++_index < _list.Count;
                }
            }

            public void Reset()
            {
                _index = -1;
            }

            public readonly void Dispose()
            {
                // No disposal needed.
            }

            private readonly ImmutableListSlice<T> _list = list;
            private int _index = -1;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        public int IndexOf(T item)
        {
            int result = _inner.IndexOf(item, _offset, _count, null);
            if (result < _offset)
            {
                return -1;
            }
            else
            {
                return result - _offset;
            }
        }

        public int IndexOf(T item, int index, int count, IEqualityComparer<T>? equalityComparer)
        {
            if (index < 0 || _count > 0 && index >= _count || _count == 0 && index != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            else if (count < 0 || index + count > _count)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            int result = _inner.IndexOf(item, _offset + index, count, equalityComparer);
            if (result < _offset)
            {
                return -1;
            }
            else
            {
                return result - _offset;
            }
        }

        public void Insert(int index, T item)
        {
            throw new NotSupportedException();
        }

        public IImmutableList<T> InsertRange(int index, IEnumerable<T> items)
        {
            if (index < 0 || index > _count) throw new ArgumentOutOfRangeException(nameof(index));
            if (items == null) throw new ArgumentNullException(nameof(items));

            int count = items.Count();
            if (count == 0)
            {
                return this;
            }

            ImmutableList<T>.Builder builder = ImmutableList.CreateBuilder<T>();
            for (int i = 0; i < index; i++)
            {
                builder.Add(_inner[_offset + i]);
            }

            builder.AddRange(items);
            for (int i = index; i < _count; i++)
            {
                builder.Add(_inner[_offset + i]);
            }
            Debug.Assert(builder.Count == _count + count);
            return builder.ToImmutable();
        }

        public int LastIndexOf(T item, int index, int count, IEqualityComparer<T>? equalityComparer)
        {
            if (index < 0 || _count > 0 && index >= _count || _count == 0 && index != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            else if (count < 0 || count > index + 1)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            int result = _inner.LastIndexOf(item, _offset + index, count, equalityComparer);
            if (result < _offset)
            {
                return -1;
            }
            else
            {
                return result - _offset;
            }
        }

        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }

        public IImmutableList<T> Remove(T value, IEqualityComparer<T>? equalityComparer)
        {
            int index = IndexOf(value, 0, _count, equalityComparer);
            if (index < 0)
            {
                return this;
            }
            else
            {
                return RemoveRange(index, 1);
            }
        }

        public IImmutableList<T> RemoveAll(Predicate<T> match)
        {
            if (match == null) throw new ArgumentNullException(nameof(match));

            ImmutableList<T>.Builder builder = ImmutableList.CreateBuilder<T>();
            foreach (T item in this)
            {
                if (!match(item)) builder.Add(item);
            }

            if (builder.Count == _count)
            {
                return this;
            }
            else
            {
                return builder.ToImmutable();
            }
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        public IImmutableList<T> RemoveRange(IEnumerable<T> items, IEqualityComparer<T>? equalityComparer)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            SortedSet<int> toRemove = new();
            foreach (T item in items)
            {
                int indexToRemove = -1;
                while (indexToRemove + 1 < _count)
                {
                    indexToRemove = IndexOf(item, indexToRemove + 1, _count - indexToRemove - 1,
                        equalityComparer);
                    if (indexToRemove < 0)
                    {
                        break;
                    }
                    else
                    {
                        toRemove.Add(indexToRemove);
                    }
                }
            }

            if (toRemove.Count == 0)
            {
                return this;
            }

            ImmutableList<T>.Builder builder = ImmutableList.CreateBuilder<T>();
            for (int i = 0; i < _count; i++)
            {
                if (!toRemove.Contains(i))
                {
                    builder.Add(_inner[_offset + i]);
                }
            }
            Debug.Assert(builder.Count == _count - toRemove.Count);
            return builder.ToImmutable();
        }

        public IImmutableList<T> RemoveRange(int index, int count)
        {
            if (index < 0 || index >= _count) throw new ArgumentOutOfRangeException(nameof(index));
            if (count < 0 || index + count > _count) throw new ArgumentOutOfRangeException(nameof(count));

            if (count == 0)
            {
                return this;
            }

            ImmutableList<T>.Builder builder = ImmutableList.CreateBuilder<T>();
            for (int i = 0; i < index; i++)
            {
                builder.Add(_inner[_offset + i]);
            }
            for (int i = index + count; i < _count; i++)
            {
                builder.Add(_inner[_offset + i]);
            }
            Debug.Assert(builder.Count == _count - count);
            return builder.ToImmutable();
        }

        public IImmutableList<T> Replace(T oldValue, T newValue, IEqualityComparer<T>? equalityComparer)
        {
            int index = IndexOf(oldValue, 0, _count, equalityComparer);
            if (index < 0)
            {
                throw new ArgumentException("Cannot find value in list.", nameof(oldValue));
            }

            return SetItem(index, newValue);
        }

        public IImmutableList<T> SetItem(int index, T value)
        {
            if (index < 0 || index >= _count) throw new ArgumentOutOfRangeException(nameof(index));

            ImmutableList<T>.Builder builder = ImmutableList.CreateBuilder<T>();
            for (int i = 0; i < _count; i++)
            {
                if (i == index)
                {
                    builder.Add(value);
                }
                else
                {
                    builder.Add(_inner[_offset + i]);
                }
            }
            return builder.ToImmutable();
        }

        IImmutableList<T> IImmutableList<T>.Add(T value)
        {
            ImmutableList<T>.Builder builder = ImmutableList.CreateBuilder<T>();
            for (int i = 0; i < _count; i++)
            {
                builder.Add(_inner[_offset + i]);
            }
            builder.Add(value);
            return builder.ToImmutable();
        }

        IImmutableList<T> IImmutableList<T>.Clear()
        {
            return ImmutableList<T>.Empty;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        IImmutableList<T> IImmutableList<T>.Insert(int index, T element)
        {
            if (index < 0 || index > _count) throw new ArgumentOutOfRangeException(nameof(index));

            ImmutableList<T>.Builder builder = ImmutableList.CreateBuilder<T>();
            for (int i = 0; i < index; i++)
            {
                builder.Add(_inner[_offset + i]);
            }
            builder.Add(element);
            for (int i = index; i < _count; i++)
            {
                builder.Add(_inner[_offset + i]);
            }
            return builder.ToImmutable();
        }

        IImmutableList<T> IImmutableList<T>.RemoveAt(int index)
        {
            return RemoveRange(index, 1);
        }

        internal IImmutableList<T> Inner => _inner;

        internal int Offset => _offset;

        private readonly IImmutableList<T> _inner;
        private readonly int _offset;
        private readonly int _count;
    }
}
