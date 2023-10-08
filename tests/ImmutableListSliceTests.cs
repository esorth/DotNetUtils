using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            Assert.AreSame(slice, slice.AddRange(new int[] { }));
            Assert.IsTrue(slice.Contains(7));
            Assert.IsFalse(slice.Contains(4));
            Assert.IsFalse(slice.Contains(10));
            Assert.AreEqual(1, slice.IndexOf(6));
            Assert.AreEqual(-1, slice.IndexOf(4));
            Assert.AreEqual(2, slice.IndexOf(7, 1, 2, null));
            Assert.AreEqual(-1, slice.IndexOf(8, 1, 2, null));
            CollectionAssert.AreEqual(new int[] { 5, 6, 1, 2, 3, 7, 8, 9 }, slice.InsertRange(2, new int[] { 1, 2, 3 }).ToArray());
            Assert.AreSame(slice, slice.InsertRange(2, new int[] { }));
            Assert.AreEqual(3, slice.LastIndexOf(8, 3, 3, null));
            Assert.AreEqual(-1, slice.LastIndexOf(5, 3, 3, null));
            CollectionAssert.AreEqual(new int[] { 5, 6, 8, 9 }, slice.Remove(7, null).ToArray());
            Assert.AreSame(slice, slice.Remove(10, null));
            CollectionAssert.AreEqual(new int[] { 5, 7, 9 }, slice.RemoveAll(x => x == 6 || x == 8).ToArray());
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

            var copyDestination = new int[7];
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
        public void ListFromImmutable()
        {
            IImmutableList<int> list = Array.ToImmutableList();
            Assert.IsTrue(list is IList<int>);
            IImmutableList<int> slice = list.ToImmutableList(5..10);

            Assert.IsTrue(slice is ImmutableListSlice<int>);
            CollectionAssert.AreEqual(new int[] { 5, 6, 7, 8, 9 }, slice.ToArray());
        }

        [TestMethod()]
        public void SliceFromSlice()
        {
            IImmutableList<int> list = Array.ToImmutableList();
            Assert.IsTrue(list is IList<int>);

            IImmutableList<int> slice = list.ToImmutableList(1..10).ToImmutableList(4..7);
            Assert.IsTrue(slice is ImmutableListSlice<int>);
            Assert.AreSame(((ImmutableListSlice<int>)slice).Inner, list);
            CollectionAssert.AreEqual(new int[] { 5, 6, 7 }, slice.ToArray());
        }

        [TestMethod()]
        public void SliceFromArray()
        {
            Assert.IsTrue(Array is IList<int>);
            IImmutableList<int> slice = Array.ToImmutableList(5..10);

            Assert.IsFalse(slice is ImmutableListSlice<int>);
            Assert.IsTrue(slice is IList<int>);
            CollectionAssert.AreEqual(new int[] { 5, 6, 7, 8, 9 }, slice.ToArray());
        }

        [TestMethod()]
        public void SliceFromStack()
        {
            Stack<int> stack = new(Array);
            Assert.IsFalse(stack is IList<int>);
            IImmutableList<int> slice = stack.ToImmutableList(5..10);

            Assert.IsFalse(slice is ImmutableListSlice<int>);
            Assert.IsTrue(slice is IList<int>);
            CollectionAssert.AreEqual(new int[] { 9, 8, 7, 6, 5 }, slice.ToArray());
        }

        [TestMethod()]
        public void SpanFromImmutableArray()
        {
            var array = Array.ToImmutableArray();
            ReadOnlySpan<int> span = array.ToImmutableSpan(5..10);

            CollectionAssert.AreEqual(new int[] { 5, 6, 7, 8, 9 }, span.ToArray());
        }

        [TestMethod()]
        public void SpanFromArray()
        {
            ReadOnlySpan<int> span = Array.ToImmutableSpan(5..10);

            CollectionAssert.AreEqual(new int[] { 5, 6, 7, 8, 9 }, span.ToArray());
        }

        [TestMethod()]
        public void SpanFromStack()
        {
            Stack<int> stack = new(Array);
            ReadOnlySpan<int> span = stack.ToImmutableSpan(5..10);

            CollectionAssert.AreEqual(new int[] { 9, 8, 7, 6, 5 }, span.ToArray());
        }
    }
}
