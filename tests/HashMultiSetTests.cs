// .Net Utils - Misc utility classes/functions for use in .Net libraries.
// Written in 2026 by Eric Orth
//
// To the extent possible under law, the author(s) have dedicated all copyright and related and
// neighboring rights to this software to the public domain worldwide. This software is distributed
// without any warranty.
//
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If
// not, see http://creativecommons.org/publicdomain/zero/1.0/.

// Collection operations are the code under test, so keep them explicitly instead of using
// collection initializers.
#pragma warning disable IDE0028

// Explicitly use properties/methods under test instead of relying on Assert methods that may
// internally rely instead on Enumerable/Linq methods.
#pragma warning disable MSTEST0037

namespace DotNetUtils.Tests
{
    [TestClass]
    public class HashMultiSetTests
    {
        [TestMethod]
        public void Constructors()
        {
            // Default constructor
            HashMultiSet<string> set1 = new();
            Assert.AreEqual(0, set1.Count);
            Assert.AreEqual(0, set1.CountUnique);

            // Constructor with items
            List<string> list = new() { "a", "b", "b", "c" };
            HashMultiSet<string> set2 = new(list);
            Assert.AreEqual(4, set2.Count);
            Assert.AreEqual(3, set2.CountUnique);
            Assert.AreEqual(1, set2.CountOf("a"));
            Assert.AreEqual(2, set2.CountOf("b"));
            Assert.AreEqual(1, set2.CountOf("c"));
            Assert.AreEqual(0, set2.CountOf("d"));

            // Constructor with comparer
            HashMultiSet<string> set3 = new(StringComparer.OrdinalIgnoreCase);
            set3.Add("a");
            set3.Add("A");
            Assert.AreEqual(2, set3.Count);
            Assert.AreEqual(1, set3.CountUnique);
            Assert.AreEqual(2, set3.CountOf("a"));
            Assert.AreEqual(2, set3.CountOf("A"));

            // Constructor with items and comparer
            List<string> list2 = new() { "a", "B", "b", "C", "c" };
            HashMultiSet<string> set4 = new(list2, StringComparer.OrdinalIgnoreCase);
            Assert.AreEqual(5, set4.Count);
            Assert.AreEqual(3, set4.CountUnique);
            Assert.AreEqual(1, set4.CountOf("a"));
            Assert.AreEqual(1, set4.CountOf("A"));
            Assert.AreEqual(2, set4.CountOf("b"));
            Assert.AreEqual(2, set4.CountOf("B"));
            Assert.AreEqual(2, set4.CountOf("c"));
            Assert.AreEqual(2, set4.CountOf("C"));
        }

        [TestMethod]
        public void Add()
        {
            HashMultiSet<string> set = new();
            set.Add("a");
            Assert.AreEqual(1, set.Count);
            Assert.AreEqual(1, set.CountUnique);
            Assert.AreEqual(1, set.CountOf("a"));

            set.Add("a");
            Assert.AreEqual(2, set.Count);
            Assert.AreEqual(1, set.CountUnique);
            Assert.AreEqual(2, set.CountOf("a"));

            set.Add("b");
            Assert.AreEqual(3, set.Count);
            Assert.AreEqual(2, set.CountUnique);
            Assert.AreEqual(2, set.CountOf("a"));
            Assert.AreEqual(1, set.CountOf("b"));
        }

        [TestMethod]
        public void AddN()
        {
            HashMultiSet<string> set = new();
            set.AddN("a", 3);
            Assert.AreEqual(3, set.Count);
            Assert.AreEqual(1, set.CountUnique);
            Assert.AreEqual(3, set.CountOf("a"));

            set.AddN("a", 2);
            Assert.AreEqual(5, set.Count);
            Assert.AreEqual(1, set.CountUnique);
            Assert.AreEqual(5, set.CountOf("a"));

            set.AddN("b", 4);
            Assert.AreEqual(9, set.Count);
            Assert.AreEqual(2, set.CountUnique);
            Assert.AreEqual(5, set.CountOf("a"));
            Assert.AreEqual(4, set.CountOf("b"));

            set.AddN("c", 0);
            Assert.AreEqual(9, set.Count);
            Assert.AreEqual(2, set.CountUnique);

            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => set.AddN("d", -1));
        }

        [TestMethod]
        public void AddRange()
        {
            HashMultiSet<string> set = new();
            set.AddRange(new[] { "a", "b", "a", "c", "b", "a" });
            Assert.AreEqual(6, set.Count);
            Assert.AreEqual(3, set.CountUnique);
            Assert.AreEqual(3, set.CountOf("a"));
            Assert.AreEqual(2, set.CountOf("b"));
            Assert.AreEqual(1, set.CountOf("c"));
        }

        [TestMethod]
        public void RemoveOne()
        {
            HashMultiSet<string> set = new(new[] { "a", "a", "a", "b" });
            Assert.IsTrue(set.RemoveOne("a"));
            Assert.AreEqual(3, set.Count);
            Assert.AreEqual(2, set.CountOf("a"));

            Assert.IsTrue(set.RemoveOne("a"));
            Assert.AreEqual(2, set.Count);
            Assert.AreEqual(1, set.CountOf("a"));

            Assert.IsTrue(set.RemoveOne("b"));
            Assert.AreEqual(1, set.Count);
            Assert.AreEqual(1, set.CountUnique);
            Assert.AreEqual(0, set.CountOf("b"));

            Assert.IsFalse(set.RemoveOne("c"));
            Assert.AreEqual(1, set.Count);

            Assert.IsTrue(set.RemoveOne("a"));
            Assert.AreEqual(0, set.Count);
            Assert.AreEqual(0, set.CountUnique);
            Assert.AreEqual(0, set.CountOf("a"));

            Assert.IsFalse(set.RemoveOne("a"));
            Assert.AreEqual(0, set.Count);
            Assert.AreEqual(0, set.CountUnique);
            Assert.AreEqual(0, set.CountOf("a"));
        }

        [TestMethod]
        public void RemoveN()
        {
            HashMultiSet<string> set = new(new[] { "a", "a", "a", "a", "a" }); // 5 'a's
            Assert.AreEqual(3, set.RemoveN("a", 3));
            Assert.AreEqual(2, set.Count);
            Assert.AreEqual(2, set.CountOf("a"));

            Assert.AreEqual(2, set.RemoveN("a", 5));
            Assert.AreEqual(0, set.Count);
            Assert.AreEqual(0, set.CountOf("a"));

            Assert.AreEqual(0, set.RemoveN("a", 1));
            Assert.AreEqual(0, set.RemoveN("b", 1));

            set.Add("c");
            Assert.AreEqual(0, set.RemoveN("c", 0));
            Assert.AreEqual(1, set.CountOf("c"));

            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => set.RemoveN("c", -1));
        }

        [TestMethod]
        public void RemoveAll() // Tests the Remove(T item) that removes all instances
        {
            HashMultiSet<string> set = new(new[] { "a", "a", "a", "b", "b" });
            Assert.AreEqual(3, set.Remove("a"));
            Assert.AreEqual(2, set.Count);
            Assert.AreEqual(1, set.CountUnique);
            Assert.AreEqual(0, set.CountOf("a"));
            Assert.AreEqual(2, set.CountOf("b"));

            Assert.AreEqual(0, set.Remove("c"));
            Assert.AreEqual(2, set.Count);

            Assert.AreEqual(2, set.Remove("b"));
            Assert.AreEqual(0, set.Count);
            Assert.AreEqual(0, set.CountUnique);
        }

        [TestMethod]
        public void ICollectionRemove()
        {
            HashMultiSet<string> set = new(new[] { "a", "a" });
            ICollection<string> collection = set;

            Assert.IsTrue(collection.Remove("a"));
            Assert.AreEqual(1, set.Count);
            Assert.AreEqual(1, set.CountOf("a"));

            Assert.IsFalse(collection.Remove("b"));

            Assert.IsTrue(collection.Remove("a"));
            Assert.AreEqual(0, set.Count);

            Assert.IsFalse(collection.Remove("a"));
        }

        [TestMethod]
        public void RemoveOneRange()
        {
            HashMultiSet<string> set = new(new[] { "a", "a", "b", "c", "c" });
            int removed = set.RemoveOneRange(new[] { "a", "c", "d", "a" });
            Assert.AreEqual(3, removed);
            Assert.AreEqual(2, set.Count);
            Assert.AreEqual(0, set.CountOf("a"));
            Assert.AreEqual(1, set.CountOf("b"));
            Assert.AreEqual(1, set.CountOf("c"));
        }

        [TestMethod]
        public void RemoveRange()
        {
            HashMultiSet<string> set = new(new[] { "a", "a", "b", "c", "c", "c" });
            int removed = set.RemoveRange(new[] { "a", "c", "d" });
            Assert.AreEqual(5, removed); // 2 'a's + 3 'c's
            Assert.AreEqual(1, set.Count);
            Assert.AreEqual(1, set.CountOf("b"));
            Assert.AreEqual(0, set.CountOf("a"));
            Assert.AreEqual(0, set.CountOf("c"));
        }

        [TestMethod]
        public void SetCount()
        {
            HashMultiSet<string> set = new();

            // Set new item
            set.SetCount("a", 3);
            Assert.AreEqual(3, set.Count);
            Assert.AreEqual(1, set.CountUnique);
            Assert.AreEqual(3, set.CountOf("a"));

            // Increase count
            set.SetCount("a", 5);
            Assert.AreEqual(5, set.Count);
            Assert.AreEqual(1, set.CountUnique);
            Assert.AreEqual(5, set.CountOf("a"));

            // Decrease count
            set.SetCount("a", 2);
            Assert.AreEqual(2, set.Count);
            Assert.AreEqual(1, set.CountUnique);
            Assert.AreEqual(2, set.CountOf("a"));

            // Set another item
            set.SetCount("b", 4);
            Assert.AreEqual(6, set.Count);
            Assert.AreEqual(2, set.CountUnique);
            Assert.AreEqual(4, set.CountOf("b"));

            // Set to zero
            set.SetCount("a", 0);
            Assert.AreEqual(4, set.Count);
            Assert.AreEqual(1, set.CountUnique);
            Assert.AreEqual(0, set.CountOf("a"));

            // Set non-existent to zero
            set.SetCount("c", 0);
            Assert.AreEqual(4, set.Count);
            Assert.AreEqual(1, set.CountUnique);

            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => set.SetCount("a", -1));
        }

        [TestMethod]
        public void Clear()
        {
            HashMultiSet<string> set = new(new[] { "a", "b", "c" });
            set.Clear();
            Assert.AreEqual(0, set.Count);
            Assert.AreEqual(0, set.CountUnique);
            Assert.IsEmpty(set.ToArray<string>());
        }

        [TestMethod]
        public void Properties()
        {
            HashMultiSet<string> set = new();
            set.AddN("a", 2);
            set.Add("b");

            Assert.IsFalse(set.IsReadOnly);
            Assert.AreEqual(3, set.Count);
            Assert.AreEqual(2, set.CountUnique);

            // Keys
            CollectionAssert.AreEquivalent(new[] { "a", "b" }, set.Keys.ToList());

            // Values
            CollectionAssert.AreEquivalent(new[] { 2, 1 }, set.Values.ToList());

            // UniqueItems
            IReadOnlySet<string> unique = set.UniqueItems;
            Assert.AreEqual(2, unique.Count);
            Assert.IsTrue(unique.Contains("a"));
            Assert.IsTrue(unique.Contains("b"));
            Assert.IsFalse(unique.Contains("c"));

            // ItemCounts
            IReadOnlyDictionary<string, int> counts = set.ItemCounts;
            Assert.AreEqual(2, counts.Count);
            Assert.AreEqual(2, counts["a"]);
            Assert.AreEqual(1, counts["b"]);

            // ItemCollections
            IReadOnlySet<IReadOnlyCollection<string>> collections = set.ItemCollections;
            Assert.AreEqual(2, collections.Count);
            var colList = collections.ToList();
            // Order is not guaranteed
            IReadOnlyCollection<string> colA = colList.Single(c => c.First() == "a");
            IReadOnlyCollection<string> colB = colList.Single(c => c.First() == "b");
            CollectionAssert.AreEqual(new[] { "a", "a" }, colA.ToArray());
            CollectionAssert.AreEqual(new[] { "b" }, colB.ToArray());
        }

        [TestMethod]
        public void IndexerAndCountOf()
        {
            HashMultiSet<string> set = new(new[] { "a", "a", "b" });
            Assert.AreEqual(2, set.CountOf("a"));
            Assert.AreEqual(1, set.CountOf("b"));
            Assert.AreEqual(0, set.CountOf("c"));

            Assert.AreEqual(2, set["a"]);
            Assert.AreEqual(1, set["b"]);
            Assert.ThrowsExactly<KeyNotFoundException>(() => set["c"]);
        }

        [TestMethod]
        public void CopyTo()
        {
            HashMultiSet<string> set = new(new[] { "a", "b", "a", "c" });
            string[] array = new string[5];
            set.CopyTo(array, 1);

            Assert.IsNull(array[0]);
            // The order is not guaranteed, so we should sort.
            var copied = array.Skip(1).ToList();
            copied.Sort();
            CollectionAssert.AreEqual(new[] { "a", "a", "b", "c" }, copied);

            // Exceptions
            Assert.ThrowsExactly<ArgumentNullException>(() => set.CopyTo(null!, 0));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => set.CopyTo(array, -1));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => set.CopyTo(array, 6));
            Assert.ThrowsExactly<ArgumentException>(() => set.CopyTo(array, 2)); // not enough space
        }

        [TestMethod]
        public void Contains()
        {
            HashMultiSet<string> set = new(new[] { "a", "a", "b" });

            // Contains(T item)
            Assert.IsTrue(set.Contains("a"));
            Assert.IsTrue(set.Contains("b"));
            Assert.IsFalse(set.Contains("c"));
            Assert.ThrowsExactly<ArgumentNullException>(() => set.Contains((string)null!));

            // Contains(T item, int count)
            Assert.IsTrue(set.Contains("a", 2));
            Assert.IsFalse(set.Contains("a", 1));
            Assert.IsTrue(set.Contains("b", 1));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => set.Contains("c", 0));
            Assert.IsFalse(set.Contains("c", 1));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => set.Contains("a", -1));

            // ContainsKey(T key)
            Assert.IsTrue(set.ContainsKey("a"));
            Assert.IsFalse(set.ContainsKey("c"));

            // Contains(IReadOnlyCollection<T> items)
            Assert.IsTrue(set.Contains(new[] { "a", "a" }));
            Assert.IsFalse(set.Contains(new[] { "a" }));
            Assert.IsFalse(set.Contains(new[] { "a", "b" })); // not a valid duplicate grouping
            Assert.IsFalse(set.Contains(new[] { "c", "c" }));
            Assert.IsFalse(set.Contains(Array.Empty<string>()));
            Assert.IsFalse(set.Contains((IReadOnlyCollection<string>)null!));
        }

        [TestMethod]
        public void TryGetValue()
        {
            HashMultiSet<string> set = new(new[] { "a", "a", "b" });
            Assert.IsTrue(set.TryGetValue("a", out int countA));
            Assert.AreEqual(2, countA);

            Assert.IsTrue(set.TryGetValue("b", out int countB));
            Assert.AreEqual(1, countB);

            Assert.IsFalse(set.TryGetValue("c", out int countC));
            Assert.AreEqual(0, countC); // default value
        }

        [TestMethod]
        public void Enumerators()
        {
            HashMultiSet<string> set = new(new[] { "a", "b", "a", "c" });

            // IEnumerator<T>
            var list = set.ToList<string>();
            list.Sort();
            CollectionAssert.AreEqual(new[] { "a", "a", "b", "c" }, list);

            // IEnumerable<KeyValuePair<T, int>>
            var counts =
                ((IEnumerable<KeyValuePair<string, int>>)set).ToDictionary(
                    item => item.Key, item => item.Value);
            Assert.HasCount(3, counts);
            Assert.AreEqual(2, counts["a"]);
            Assert.AreEqual(1, counts["b"]);
            Assert.AreEqual(1, counts["c"]);

            // IEnumerable<IReadOnlyCollection<T>>
            var collections = ((IEnumerable<IReadOnlyCollection<string>>)set).ToList();
            Assert.HasCount(3, collections);
            // order not guaranteed
            IReadOnlyCollection<string> colA = collections.Single(c => c.First() == "a");
            IReadOnlyCollection<string> colB = collections.Single(c => c.First() == "b");
            IReadOnlyCollection<string> colC = collections.Single(c => c.First() == "c");
            CollectionAssert.AreEqual(new[] { "a", "a" }, colA.ToArray());
            CollectionAssert.AreEqual(new[] { "b" }, colB.ToArray());
            CollectionAssert.AreEqual(new[] { "c" }, colC.ToArray());
        }

        [TestMethod]
        public void SetOperations()
        {
            HashMultiSet<string> set = new();
            set.AddN("a", 2);
            set.Add("b");

            List<IReadOnlyCollection<string>> other1 = new()
            {
                new[] { "a", "a" },
                new[] { "b" }
            };
            List<IReadOnlyCollection<string>> other2 = new()
            {
                new[] { "a", "a" },
                new[] { "b" },
                new[] { "c" }
            };
            List<IReadOnlyCollection<string>> other3 = new()
            {
                new[] { "a", "a" }
            };
            List<IReadOnlyCollection<string>> other4 = new()
            {
                new[] { "a", "a" },
                new[] { "d" }
            };
            List<IReadOnlyCollection<string>> other5 = new()
            {
                new[] { "a" } // different count
            };
            List<IReadOnlyCollection<string>> other6 = new()
            {
                new[] { "a", "b" }, // invalid grouping
                new[] { "a", "a" },
                new[] { "b" }
            };

            // SetEquals
            Assert.IsTrue(set.SetEquals(other1));
            Assert.IsFalse(set.SetEquals(other2));
            Assert.IsFalse(set.SetEquals(other3));

            // IsSubsetOf
            Assert.IsTrue(set.IsSubsetOf(other1));
            Assert.IsTrue(set.IsSubsetOf(other2));
            Assert.IsFalse(set.IsSubsetOf(other3));
            Assert.IsFalse(set.IsSubsetOf(other4));
            Assert.IsFalse(set.IsSubsetOf(other5));

            // IsProperSubsetOf
            Assert.IsFalse(set.IsProperSubsetOf(other1));
            Assert.IsTrue(set.IsProperSubsetOf(other2));
            Assert.IsFalse(set.IsProperSubsetOf(other3));

            // IsSupersetOf
            Assert.IsTrue(set.IsSupersetOf(other1));
            Assert.IsFalse(set.IsSupersetOf(other2));
            Assert.IsTrue(set.IsSupersetOf(other3));
            Assert.IsFalse(set.IsSupersetOf(other4));
            Assert.IsFalse(set.IsSupersetOf(other5));

            // IsProperSupersetOf
            Assert.IsFalse(set.IsProperSupersetOf(other1));
            Assert.IsFalse(set.IsProperSupersetOf(other2));
            Assert.IsTrue(set.IsProperSupersetOf(other3));

            // Overlaps
            Assert.IsTrue(set.Overlaps(other1));
            Assert.IsTrue(set.Overlaps(other2));
            Assert.IsTrue(set.Overlaps(other3));
            Assert.IsTrue(set.Overlaps(other4));
            Assert.IsFalse(set.Overlaps(other5));
            Assert.IsFalse(set.Overlaps(new[] { new[] { "d" } }));

            // with invalid grouping
            Assert.IsFalse(set.SetEquals(other6));
            Assert.IsTrue(set.IsSubsetOf(other6));
            Assert.IsFalse(set.IsSupersetOf(other6));
            Assert.IsTrue(set.Overlaps(other6));
        }

        [TestMethod]
        public void UniqueItemsView()
        {
            HashMultiSet<string> set = new(new[] { "a", "a", "b" });
            IReadOnlySet<string> uniqueView = set.UniqueItems;

            Assert.AreEqual(2, uniqueView.Count);
            Assert.IsTrue(uniqueView.Contains("a"));
            Assert.IsTrue(uniqueView.Contains("b"));
            Assert.IsFalse(uniqueView.Contains("c"));

            var list = uniqueView.ToList();
            list.Sort();
            CollectionAssert.AreEqual(new[] { "a", "b" }, list);

            // Set operations
            Assert.IsTrue(uniqueView.SetEquals(new[] { "b", "a" }));
            Assert.IsFalse(uniqueView.SetEquals(new[] { "a" }));

            Assert.IsTrue(uniqueView.IsSubsetOf(new[] { "a", "b", "c" }));
            Assert.IsFalse(uniqueView.IsSubsetOf(new[] { "a", "c" }));

            Assert.IsTrue(uniqueView.IsSupersetOf(new[] { "b" }));
            Assert.IsFalse(uniqueView.IsSupersetOf(new[] { "c", "b" }));

            Assert.IsTrue(uniqueView.IsProperSupersetOf(new[] { "a" }));
            Assert.IsFalse(uniqueView.IsProperSupersetOf(new[] { "a", "b" }));

            Assert.IsTrue(uniqueView.IsProperSubsetOf(new[] { "a", "b", "c" }));
            Assert.IsFalse(uniqueView.IsProperSubsetOf(new[] { "a", "b" }));

            Assert.IsTrue(uniqueView.Overlaps(new[] { "b", "d" }));
            Assert.IsFalse(uniqueView.Overlaps(new[] { "d", "e" }));
        }
    }
}
