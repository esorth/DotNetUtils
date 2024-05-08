// .Net Utils - Misc utility classes/functions for use in .Net libraries.
// Written in 2024 by Eric Orth
//
// To the extent possible under law, the author(s) have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see http://creativecommons.org/publicdomain/zero/1.0/.

namespace DotNetUtils.Tests
{
    [TestClass()]
    public class RangeExtensionsTests
    {
        [TestMethod()]
        public void Add()
        {
            Index idx = 5;
            Index result = idx.Add(3);

            Assert.AreEqual(8, result);
        }

        [TestMethod()]
        public void AddFromEnd()
        {
            Index idx = ^5;
            Index result = idx.Add(3);

            Assert.AreEqual(^2, result);
        }

        [TestMethod()]
        public void LessThan()
        {
            Index i1 = 5;
            Index i2 = 10;
            Index i3 = ^5;
            Index i4 = ^10;

            Assert.IsFalse(i1.LessThan(i1));
            Assert.IsTrue(i1.LessThan(i2));
            Assert.IsFalse(i2.LessThan(i1));

            Assert.IsFalse(i3.LessThan(i3));
            Assert.IsTrue(i4.LessThan(i3));
            Assert.IsFalse(i3.LessThan(i4));

            Assert.IsNull(i1.LessThan(i3));
        }

        [TestMethod()]
        public void NestedContains()
        {
            Range r = 5..10;

            Assert.IsTrue(r.NestedContains(3));
            Assert.IsFalse(r.NestedContains(7));
            Assert.IsNull(r.NestedContains(^2));
        }

        [TestMethod()]
        public void NestedContainsFromEnd()
        {
            Range r = ^10..^5;

            Assert.IsTrue(r.NestedContains(^3));
            Assert.IsFalse(r.NestedContains(^7));
            Assert.IsNull(r.NestedContains(2));
        }

        [TestMethod()]
        public void NestedContainsMixed()
        {
            Range r = 5..^5;

            Assert.IsNull(r.NestedContains(^3));
            Assert.IsNull(r.NestedContains(2));
        }

        [TestMethod()]
        public void Unnest()
        {
            Range r = 5..10;

            Assert.AreEqual(7, r.UnNest(2));
            Assert.AreEqual(8, r.UnNest(^2));
            Assert.AreEqual(6..9, r.UnNest(1..4));
            Assert.AreEqual(7..8, r.UnNest(^3..^2));
            Assert.AreEqual(5..7, r.UnNest(..2));
            Assert.AreEqual(8..10, r.UnNest(3..));
        }

        [TestMethod()]
        public void UnnestFromEnd()
        {
            Range r = ^10..^5;

            Assert.AreEqual(^8, r.UnNest(2));
            Assert.AreEqual(^7, r.UnNest(^2));
            Assert.AreEqual(^9..^6, r.UnNest(1..4));
            Assert.AreEqual(^8..^7, r.UnNest(^3..^2));
            Assert.AreEqual(^10..^8, r.UnNest(..2));
            Assert.AreEqual(^7..^5, r.UnNest(3..));
        }

        [TestMethod()]
        public void UnnestFromMixed()
        {
            Range r = 7..^5;

            Assert.AreEqual(9, r.UnNest(2));
            Assert.AreEqual(^7, r.UnNest(^2));
            Assert.AreEqual(8..11, r.UnNest(1..4));
            Assert.AreEqual(^8..^7, r.UnNest(^3..^2));
            Assert.AreEqual(7..9, r.UnNest(..2));
            Assert.AreEqual(10..^5, r.UnNest(3..));
        }

        [TestMethod()]
        public void Parse()
        {
            Assert.AreEqual(1..5, RangeExtensions.Parse("1..5"));
            Assert.AreEqual(1..5, RangeExtensions.Parse("  \t+1  .. 05\n"));
            Assert.AreEqual(6..^7, RangeExtensions.Parse("6..^7"));
            Assert.AreEqual(^8..^6, RangeExtensions.Parse("^8..^6"));
            Assert.AreEqual(3..4, RangeExtensions.Parse("3"));
            Assert.AreEqual(^11..^10, RangeExtensions.Parse("^11"));
            Assert.AreEqual(.., RangeExtensions.Parse(".."));
            Assert.AreEqual(..5, RangeExtensions.Parse("..5"));
            Assert.AreEqual(6.., RangeExtensions.Parse("6.."));
        }

        [TestMethod()]
        public void TryParse()
        {
            Range parsed;

            Assert.IsTrue(RangeExtensions.TryParse("1..5", out parsed));
            Assert.AreEqual(1..5, parsed);

            Assert.IsTrue(RangeExtensions.TryParse("  \t+1  .. 05\n", out parsed));
            Assert.AreEqual(1..5, parsed);

            Assert.IsTrue(RangeExtensions.TryParse("6..^7", out parsed));
            Assert.AreEqual(6..^7, parsed);

            Assert.IsTrue(RangeExtensions.TryParse("^8..^6", out parsed));
            Assert.AreEqual(^8..^6, parsed);

            Assert.IsTrue(RangeExtensions.TryParse("3", out parsed));
            Assert.AreEqual(3..4, parsed);

            Assert.IsTrue(RangeExtensions.TryParse("^11", out parsed));
            Assert.AreEqual(^11..^10, parsed);

            Assert.IsTrue(RangeExtensions.TryParse("..", out parsed));
            Assert.AreEqual(.., parsed);

            Assert.IsTrue(RangeExtensions.TryParse("5..", out parsed));
            Assert.AreEqual(5.., parsed);

            Assert.IsTrue(RangeExtensions.TryParse("..5", out parsed));
            Assert.AreEqual(..5, parsed);

            Assert.IsFalse(RangeExtensions.TryParse("", out _));
            Assert.IsFalse(RangeExtensions.TryParse("abc", out _));
            Assert.IsFalse(RangeExtensions.TryParse("1..2..3", out _));
            Assert.IsFalse(RangeExtensions.TryParse("1..abc", out _));
            Assert.IsFalse(RangeExtensions.TryParse("-1..4", out _));
            Assert.IsFalse(RangeExtensions.TryParse("5..4", out _));
            Assert.IsFalse(RangeExtensions.TryParse("4..4", out _));
            Assert.IsFalse(RangeExtensions.TryParse("^4..^4", out _));
            Assert.IsFalse(RangeExtensions.TryParse("^3..^4", out _));
            Assert.IsFalse(RangeExtensions.TryParse("^", out _));
            Assert.IsFalse(RangeExtensions.TryParse("5..^", out _));
            Assert.IsFalse(RangeExtensions.TryParse("1.2", out _));
            Assert.IsFalse(RangeExtensions.TryParse("-5", out _));
        }
    }
}
