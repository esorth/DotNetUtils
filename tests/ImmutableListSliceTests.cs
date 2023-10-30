// .Net Utils - Misc utility classes/functions for use in .Net libraries.
// Written in 2023 by Eric Orth
//
// To the extent possible under law, the author(s) have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see http://creativecommons.org/publicdomain/zero/1.0/.

using System.Collections.Immutable;

namespace DotNetUtils.Tests
{
    [TestClass()]
    public class ImmutableListSliceTests
    {
        private static readonly int[] Array = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 };

        [TestMethod()]
        public void WrapperImmutableListMethods()
        {
            IImmutableList<int> list = Array.ToImmutableList();
            IImmutableList<int> slice = new ImmutableListSlice<int>(list, 5..10);

            Assert.AreEqual(5, slice.Count);
            CollectionAssert.AreEqual(new int[] { 5, 6, 7, 8, 9 }, slice.ToArray());
            Assert.AreEqual(6, slice[1]);
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => slice[-1]);
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => slice[5]);
            CollectionAssert.AreEqual(new int[] { 5, 6, 7, 8, 9, 1, 2, 3 }, slice.AddRange(new int[] { 1, 2, 3 }).ToArray());
            Assert.AreSame(slice, slice.AddRange(System.Array.Empty<int>()));
            Assert.IsTrue(slice.Contains(7));
            Assert.IsFalse(slice.Contains(4));
            Assert.IsFalse(slice.Contains(10));
            Assert.AreEqual(1, slice.IndexOf(6));
            Assert.AreEqual(-1, slice.IndexOf(4));
            Assert.AreEqual(2, slice.IndexOf(7, 1, 2, null));
            Assert.AreEqual(-1, slice.IndexOf(8, 1, 2, null));
            CollectionAssert.AreEqual(new int[] { 5, 6, 1, 2, 3, 7, 8, 9 }, slice.InsertRange(2, new int[] { 1, 2, 3 }).ToArray());
            Assert.AreSame(slice, slice.InsertRange(2, System.Array.Empty<int>()));
            Assert.AreEqual(3, slice.LastIndexOf(8, 3, 3, null));
            Assert.AreEqual(-1, slice.LastIndexOf(5, 3, 3, null));
            CollectionAssert.AreEqual(new int[] { 5, 6, 8, 9 }, slice.Remove(7, null).ToArray());
            Assert.AreSame(slice, slice.Remove(10, null));
            CollectionAssert.AreEqual(new int[] { 5, 7, 9 }, slice.RemoveAll(x => x is 6 or 8).ToArray());
            Assert.AreSame(slice, slice.RemoveAll(x => x < 5));
            CollectionAssert.AreEqual(new int[] { 6, 8 }, slice.RemoveRange(new int[] { 5, 7, 9, 10 }).ToArray());
            Assert.AreSame(slice, slice.RemoveRange(new int[] { 1, 2, 10 }));
            CollectionAssert.AreEqual(new int[] { 5, 6, 6, 8, 9 }, slice.Replace(7, 6, null).ToArray());
            CollectionAssert.AreEqual(new int[] { 5, 4, 7, 8, 9 }, slice.SetItem(1, 4).ToArray());
            CollectionAssert.AreEqual(new int[] { 5, 6, 7, 8, 9, 12 }, slice.Add(12).ToArray());
            CollectionAssert.AreEqual(new int[] { 2, 5, 6, 7, 8, 9 }, slice.Insert(0, 2).ToArray());
            CollectionAssert.AreEqual(new int[] { 5, 6, 7, 9 }, slice.RemoveAt(3).ToArray());
        }

        [TestMethod()]
        public void WrapperListMethods()
        {
            IImmutableList<int> list = Array.ToImmutableList();
            IList<int> slice = new ImmutableListSlice<int>(list, 5..10);

            Assert.AreEqual(5, slice.Count);
            CollectionAssert.AreEqual(new int[] { 5, 6, 7, 8, 9 }, slice.ToArray());
            Assert.AreEqual(6, slice[1]);
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => slice[-1]);
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => slice[5]);
            Assert.IsTrue(slice.Contains(7));
            Assert.IsFalse(slice.Contains(4));
            Assert.IsFalse(slice.Contains(10));
            Assert.AreEqual(1, slice.IndexOf(6));
            Assert.AreEqual(-1, slice.IndexOf(4));

            int[] copyDestination = new int[7];
            slice.CopyTo(copyDestination, 1);
            CollectionAssert.AreEqual(new int[] { 0, 5, 6, 7, 8, 9, 0 }, copyDestination);
        }

        [TestMethod()]
        public void FullRangeListFromImmutable()
        {
            IImmutableList<int> list = Array.ToImmutableList();
            Assert.IsTrue(list is IList<int>);
            Assert.AreSame(list, list.ToImmutableList(..));
        }

        [TestMethod()]
        public void EmptyListFromImmutable()
        {
            IImmutableList<int> list = Array.ToImmutableList();
            Assert.IsTrue(list is IList<int>);
            Assert.AreEqual(0, list.ToImmutableList(4..4).Count);
            Assert.AreEqual(0, list.ToImmutableList(8..4).Count);
        }

        [TestMethod()]
        public void ListFromImmutable()
        {
            IImmutableList<int> list = Array.ToImmutableList();
            Assert.IsTrue(list is IList<int>);
            IImmutableList<int> slice = list.ToImmutableList(5..10);

            Assert.IsTrue(slice is ImmutableListSlice<int>);
            CollectionAssert.AreEqual(new int[] { 5, 6, 7, 8, 9 }, slice.ToArray());
        }

        [TestMethod()]
        public void ListFromImmutableOutOfBounds()
        {
            IImmutableList<int> list = Array.ToImmutableList();
            Assert.IsTrue(list is IList<int>);

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => list.ToImmutableList(-1..));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => list.ToImmutableList(..^-1));
        }

        [TestMethod()]
        public void ListFromWrapped()
        {
            IImmutableList<int> list = Array.ToImmutableList();
            Assert.IsTrue(list is IList<int>);

            IImmutableList<int> slice = list.ToImmutableList(1..10).ToImmutableList(4..7);
            Assert.IsTrue(slice is ImmutableListSlice<int>);
            Assert.AreSame(((ImmutableListSlice<int>)slice).Inner, list);
            CollectionAssert.AreEqual(new int[] { 5, 6, 7 }, slice.ToArray());
        }

        [TestMethod()]
        public void ListFromArray()
        {
            Assert.IsTrue(Array is IList<int>);
            IImmutableList<int> slice = Array.ToImmutableList(5..10);

            Assert.IsFalse(slice is ImmutableListSlice<int>);
            Assert.IsTrue(slice is IList<int>);
            CollectionAssert.AreEqual(new int[] { 5, 6, 7, 8, 9 }, slice.ToArray());
        }

        [TestMethod()]
        public void ListFromArrayOutOfBounds()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => Array.ToImmutableList(-1..));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => Array.ToImmutableList(..^-1));
        }

        [TestMethod()]
        public void EmptyListFromArray()
        {
            Assert.AreEqual(0, Array.ToImmutableList(4..4).Count);
            Assert.AreEqual(0, Array.ToImmutableList(8..4).Count);
        }

        [TestMethod()]
        public void ListFromStack()
        {
            Stack<int> stack = new(Array);
            Assert.IsFalse(stack is IList<int>);
            IImmutableList<int> slice = stack.ToImmutableList(5..10);

            Assert.IsFalse(slice is ImmutableListSlice<int>);
            Assert.IsTrue(slice is IList<int>);
            CollectionAssert.AreEqual(new int[] { 9, 8, 7, 6, 5 }, slice.ToArray());
        }

        [TestMethod()]
        public void ListFromStackOutOfBounds()
        {
            Stack<int> stack = new(Array);
            Assert.IsFalse(stack is IList<int>);

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => stack.ToImmutableList(-1..));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => stack.ToImmutableList(..^-1));
        }

        [TestMethod()]
        public void EmptyListFromStack()
        {
            Stack<int> stack = new(Array);
            Assert.IsFalse(stack is IList<int>);

            Assert.AreEqual(0, stack.ToImmutableList(4..4).Count);
            Assert.AreEqual(0, stack.ToImmutableList(8..4).Count);
        }

        [TestMethod()]
        public void SpanFromImmutableArray()
        {
            var array = Array.ToImmutableArray();
            ReadOnlySpan<int> span = array.ToImmutableSpan(5..10);

            CollectionAssert.AreEqual(new int[] { 5, 6, 7, 8, 9 }, span.ToArray());
        }

        [TestMethod()]
        public void SpanFromImmutableArrayOutOfBounds()
        {
            var array = Array.ToImmutableArray();

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => array.ToImmutableSpan(-1..));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => array.ToImmutableSpan(..^-1));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => array.ToImmutableSpan(8..4));
        }

        [TestMethod()]
        public void EmptySpanFromImmutableArray()
        {
            var array = Array.ToImmutableArray();
            Assert.IsTrue(array.ToImmutableSpan(4..4).IsEmpty);
        }

        [TestMethod()]
        public void SpanFromArray()
        {
            ReadOnlySpan<int> span = Array.ToImmutableSpan(5..10);

            CollectionAssert.AreEqual(new int[] { 5, 6, 7, 8, 9 }, span.ToArray());
        }

        [TestMethod()]
        public void SpanFromArrayOutOfBounds()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => Array.ToImmutableSpan(-1..));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => Array.ToImmutableSpan(..^-1));
        }

        [TestMethod()]
        public void EmptySpanFromArray()
        {
            Assert.IsTrue(Array.ToImmutableSpan(4..4).IsEmpty);
            Assert.IsTrue(Array.ToImmutableSpan(8..4).IsEmpty);
        }

        [TestMethod()]
        public void SpanFromStack()
        {
            Stack<int> stack = new(Array);
            ReadOnlySpan<int> span = stack.ToImmutableSpan(5..10);

            CollectionAssert.AreEqual(new int[] { 9, 8, 7, 6, 5 }, span.ToArray());
        }

        [TestMethod()]
        public void SpanFromStackOutOfBounds()
        {
            Stack<int> stack = new(Array);

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => stack.ToImmutableSpan(-1..));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => stack.ToImmutableSpan(..^-1));
        }

        [TestMethod()]
        public void EmptySpanFromStack()
        {
            Stack<int> stack = new(Array);
            Assert.IsTrue(stack.ToImmutableSpan(4..4).IsEmpty);
            Assert.IsTrue(stack.ToImmutableSpan(8..4).IsEmpty);
        }
    }
}
