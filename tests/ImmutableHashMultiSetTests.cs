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

namespace DotNetUtils.Tests;

[TestClass]
public class ImmutableHashMultiSetTests
{
    [TestMethod]
    public void Creation()
    {
        // ImmutableHashMultiSet.Empty<T>()
        var set1 = ImmutableHashMultiSet.Empty<string>();
        Assert.AreEqual(0, set1.Count);
        Assert.AreEqual(0, set1.CountUnique);

        // ImmutableHashMultiSet<T>.Empty
        ImmutableHashMultiSet<string> set2 = ImmutableHashMultiSet<string>.Empty;
        Assert.AreSame(set1, set2);

        // ImmutableHashMultiSet.Create<T>()
        var set3 = ImmutableHashMultiSet.Create<string>();
        Assert.AreSame(set1, set3);

        // ImmutableHashMultiSet.Create<T>(comparer)
        var set4 = ImmutableHashMultiSet.Create<string>(StringComparer.OrdinalIgnoreCase);
        Assert.AreEqual(0, set4.Count);
        Assert.AreEqual(0, set4.CountUnique);
        ImmutableHashMultiSet<string> set4_2 = set4.Add("a").Add("A");
        Assert.AreEqual(2, set4_2.Count);
        Assert.AreEqual(1, set4_2.CountUnique);

        // ImmutableHashMultiSet.CreateRange(items)
        string[] list = new[] { "a", "b", "b", "c" };
        var set5 = ImmutableHashMultiSet.CreateRange(list);
        Assert.AreEqual(4, set5.Count);
        Assert.AreEqual(3, set5.CountUnique);
        Assert.AreEqual(1, set5.CountOf("a"));
        Assert.AreEqual(2, set5.CountOf("b"));
        Assert.AreEqual(1, set5.CountOf("c"));

        // ImmutableHashMultiSet.CreateRange(comparer, items)
        string[] list2 = new[] { "a", "B", "b" };
        var set6 = ImmutableHashMultiSet.CreateRange(StringComparer.OrdinalIgnoreCase, list2);
        Assert.AreEqual(3, set6.Count);
        Assert.AreEqual(2, set6.CountUnique);
        Assert.AreEqual(1, set6.CountOf("a"));
        Assert.AreEqual(2, set6.CountOf("b"));
        Assert.AreEqual(2, set6.CountOf("B"));
    }

    [TestMethod]
    public void Add()
    {
        var set = ImmutableHashMultiSet.Create<string>();
        ImmutableHashMultiSet<string> set1 = set.Add("a");
        Assert.AreEqual(0, set.Count); // original is unchanged
        Assert.AreEqual(1, set1.Count);
        Assert.AreEqual(1, set1.CountUnique);
        Assert.AreEqual(1, set1.CountOf("a"));

        ImmutableHashMultiSet<string> set2 = set1.Add("a");
        Assert.AreEqual(1, set1.Count); // original is unchanged
        Assert.AreEqual(2, set2.Count);
        Assert.AreEqual(1, set2.CountUnique);
        Assert.AreEqual(2, set2.CountOf("a"));

        ImmutableHashMultiSet<string> set3 = set2.Add("b");
        Assert.AreEqual(2, set2.Count); // original is unchanged
        Assert.AreEqual(3, set3.Count);
        Assert.AreEqual(2, set3.CountUnique);
        Assert.AreEqual(2, set3.CountOf("a"));
        Assert.AreEqual(1, set3.CountOf("b"));
    }

    [TestMethod]
    public void AddN()
    {
        var set = ImmutableHashMultiSet.Create<string>();
        ImmutableHashMultiSet<string> set1 = set.AddN("a", 3);
        Assert.AreEqual(0, set.Count);
        Assert.AreEqual(3, set1.Count);
        Assert.AreEqual(1, set1.CountUnique);
        Assert.AreEqual(3, set1.CountOf("a"));

        ImmutableHashMultiSet<string> set2 = set1.AddN("a", 2);
        Assert.AreEqual(3, set1.Count);
        Assert.AreEqual(5, set2.Count);
        Assert.AreEqual(1, set2.CountUnique);
        Assert.AreEqual(5, set2.CountOf("a"));

        ImmutableHashMultiSet<string> set3 = set2.AddN("b", 4);
        Assert.AreEqual(5, set2.Count);
        Assert.AreEqual(9, set3.Count);
        Assert.AreEqual(2, set3.CountUnique);
        Assert.AreEqual(5, set3.CountOf("a"));
        Assert.AreEqual(4, set3.CountOf("b"));

        ImmutableHashMultiSet<string> set4 = set3.AddN("c", 0);
        Assert.AreSame(set3, set4);
        Assert.AreEqual(9, set3.Count);

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => set.AddN("d", -1));
    }

    [TestMethod]
    public void AddRange()
    {
        var set = ImmutableHashMultiSet.Create<string>();
        ImmutableHashMultiSet<string> set1 = set.AddRange(new[] { "a", "b", "a", "c", "b", "a" });
        Assert.AreEqual(0, set.Count);
        Assert.AreEqual(6, set1.Count);
        Assert.AreEqual(3, set1.CountUnique);
        Assert.AreEqual(3, set1.CountOf("a"));
        Assert.AreEqual(2, set1.CountOf("b"));
        Assert.AreEqual(1, set1.CountOf("c"));
    }

    [TestMethod]
    public void RemoveOne()
    {
        var set = ImmutableHashMultiSet.CreateRange(new[] { "a", "a", "a", "b" });
        ImmutableHashMultiSet<string> set1 = set.RemoveOne("a");
        Assert.AreEqual(4, set.Count);
        Assert.AreEqual(3, set1.Count);
        Assert.AreEqual(2, set1.CountOf("a"));

        ImmutableHashMultiSet<string> set2 = set1.RemoveOne("a");
        Assert.AreEqual(2, set2.Count);
        Assert.AreEqual(1, set2.CountOf("a"));

        ImmutableHashMultiSet<string> set3 = set2.RemoveOne("b");
        Assert.AreEqual(1, set3.Count);
        Assert.AreEqual(1, set3.CountUnique);
        Assert.AreEqual(0, set3.CountOf("b"));

        ImmutableHashMultiSet<string> set4 = set3.RemoveOne("c");
        Assert.AreSame(set3, set4);
        Assert.AreEqual(1, set4.Count);

        ImmutableHashMultiSet<string> set5 = set4.RemoveOne("a");
        Assert.AreEqual(0, set5.Count);
        Assert.AreEqual(0, set5.CountUnique);
        Assert.AreEqual(0, set5.CountOf("a"));

        ImmutableHashMultiSet<string> set6 = set5.RemoveOne("a");
        Assert.AreSame(set5, set6);
        Assert.AreEqual(0, set6.Count);
    }

    [TestMethod]
    public void RemoveN()
    {
        var set = ImmutableHashMultiSet.CreateRange(new[] { "a", "a", "a", "a", "a" }); // 5 'a's
        ImmutableHashMultiSet<string> set1 = set.RemoveN("a", 3);
        Assert.AreEqual(2, set1.Count);
        Assert.AreEqual(2, set1.CountOf("a"));

        ImmutableHashMultiSet<string> set2 = set1.RemoveN("a", 5);
        Assert.AreEqual(0, set2.Count);
        Assert.AreEqual(0, set2.CountOf("a"));

        ImmutableHashMultiSet<string> set3 = set2.RemoveN("a", 1);
        Assert.AreSame(set2, set3);
        ImmutableHashMultiSet<string> set4 = set2.RemoveN("b", 1);
        Assert.AreSame(set2, set4);

        ImmutableHashMultiSet<string> set5 = set.Add("c");
        ImmutableHashMultiSet<string> set6 = set5.RemoveN("c", 0);
        Assert.AreSame(set5, set6);
        Assert.AreEqual(1, set6.CountOf("c"));

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => set.RemoveN("c", -1));
    }

    [TestMethod]
    public void RemoveAll() // Tests the Remove(T item) that removes all instances
    {
        var set = ImmutableHashMultiSet.CreateRange(new[] { "a", "a", "a", "b", "b" });
        ImmutableHashMultiSet<string> set1 = set.Remove("a");
        Assert.AreEqual(2, set1.Count);
        Assert.AreEqual(1, set1.CountUnique);
        Assert.AreEqual(0, set1.CountOf("a"));
        Assert.AreEqual(2, set1.CountOf("b"));

        ImmutableHashMultiSet<string> set2 = set1.Remove("c");
        Assert.AreSame(set1, set2);
        Assert.AreEqual(2, set2.Count);

        ImmutableHashMultiSet<string> set3 = set2.Remove("b");
        Assert.AreEqual(0, set3.Count);
        Assert.AreEqual(0, set3.CountUnique);
    }

    [TestMethod]
    public void IImmutableCollectionRemove()
    {
        IImmutableCollection<string> set = ImmutableHashMultiSet.CreateRange(new[] { "a", "a" });

        IImmutableCollection<string> set1 = set.Remove("a");
        Assert.AreEqual(1, set1.Count);
        Assert.AreEqual(1, ((IReadOnlyMultiSet<string>)set1).CountOf("a"));

        IImmutableCollection<string> set2 = set1.Remove("b");
        Assert.AreSame(set1, set2);

        IImmutableCollection<string> set3 = set1.Remove("a");
        Assert.AreEqual(0, set3.Count);

        IImmutableCollection<string> set4 = set3.Remove("a");
        Assert.AreSame(set3, set4);
    }

    [TestMethod]
    public void RemoveOneRange()
    {
        var set = ImmutableHashMultiSet.CreateRange(new[] { "a", "a", "b", "c", "c" });
        ImmutableHashMultiSet<string> set1 = set.RemoveOneRange(new[] { "a", "c", "d", "a" });
        Assert.AreEqual(2, set1.Count);
        Assert.AreEqual(0, set1.CountOf("a"));
        Assert.AreEqual(1, set1.CountOf("b"));
        Assert.AreEqual(1, set1.CountOf("c"));
    }

    [TestMethod]
    public void RemoveRange()
    {
        var set = ImmutableHashMultiSet.CreateRange(new[] { "a", "a", "b", "c", "c", "c" });
        ImmutableHashMultiSet<string> set1 = set.RemoveRange(new[] { "a", "c", "d" });
        Assert.AreEqual(1, set1.Count);
        Assert.AreEqual(1, set1.CountOf("b"));
        Assert.AreEqual(0, set1.CountOf("a"));
        Assert.AreEqual(0, set1.CountOf("c"));
    }

    [TestMethod]
    public void SetCount()
    {
        var set = ImmutableHashMultiSet.Create<string>();

        // Set new item
        ImmutableHashMultiSet<string> set1 = set.SetCount("a", 3);
        Assert.AreEqual(3, set1.Count);
        Assert.AreEqual(1, set1.CountUnique);
        Assert.AreEqual(3, set1.CountOf("a"));

        // Increase count
        ImmutableHashMultiSet<string> set2 = set1.SetCount("a", 5);
        Assert.AreEqual(5, set2.Count);
        Assert.AreEqual(1, set2.CountUnique);
        Assert.AreEqual(5, set2.CountOf("a"));

        // Decrease count
        ImmutableHashMultiSet<string> set3 = set2.SetCount("a", 2);
        Assert.AreEqual(2, set3.Count);
        Assert.AreEqual(1, set3.CountUnique);
        Assert.AreEqual(2, set3.CountOf("a"));

        // Set another item
        ImmutableHashMultiSet<string> set4 = set3.SetCount("b", 4);
        Assert.AreEqual(6, set4.Count);
        Assert.AreEqual(2, set4.CountUnique);
        Assert.AreEqual(4, set4.CountOf("b"));

        // Set to zero
        ImmutableHashMultiSet<string> set5 = set4.SetCount("a", 0);
        Assert.AreEqual(4, set5.Count);
        Assert.AreEqual(1, set5.CountUnique);
        Assert.AreEqual(0, set5.CountOf("a"));

        // Set non-existent to zero
        ImmutableHashMultiSet<string> set6 = set5.SetCount("c", 0);
        Assert.AreSame(set5, set6);
        Assert.AreEqual(4, set6.Count);

        // Set to same
        ImmutableHashMultiSet<string> set7 = set5.SetCount("b", 4);
        Assert.AreSame(set5, set7);

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => set.SetCount("a", -1));
    }

    [TestMethod]
    public void Clear()
    {
        var set = ImmutableHashMultiSet.CreateRange(new[] { "a", "b", "c" });
        ImmutableHashMultiSet<string> set1 = set.Clear();
        Assert.AreEqual(0, set1.Count);
        Assert.AreEqual(0, set1.CountUnique);
        Assert.IsEmpty(set1.ToArray<string>());
        Assert.AreSame(ImmutableHashMultiSet<string>.Empty, set1);

        ImmutableHashMultiSet<string> setWithComparer =
            ImmutableHashMultiSet.Create<string>(StringComparer.OrdinalIgnoreCase).Add("a");
        ImmutableHashMultiSet<string> cleared = setWithComparer.Clear();
        Assert.AreEqual(0, cleared.Count);
        // check comparer is kept
        ImmutableHashMultiSet<string> final = cleared.Add("b").Add("B");
        Assert.AreEqual(1, final.CountUnique);
    }

    [TestMethod]
    public void WithComparer()
    {
        var set = ImmutableHashMultiSet.CreateRange(new[] { "a", "A", "b" });
        Assert.AreEqual(3, set.Count);
        Assert.AreEqual(3, set.CountUnique);

        ImmutableHashMultiSet<string> set2 = set.WithComparer(StringComparer.OrdinalIgnoreCase);
        Assert.AreEqual(3, set2.Count);
        Assert.AreEqual(2, set2.CountUnique);
        Assert.AreEqual(2, set2.CountOf("a"));
        Assert.AreEqual(1, set2.CountOf("b"));

        // No change
        ImmutableHashMultiSet<string> set3 = set.WithComparer(null);
        Assert.AreSame(set, set3);

        ImmutableHashMultiSet<string> set4 = set2.WithComparer(StringComparer.OrdinalIgnoreCase);
        Assert.AreSame(set2, set4);
    }

    [TestMethod]
    public void Properties()
    {
        ImmutableHashMultiSet<string> set = ImmutableHashMultiSet.Create<string>().AddN("a", 2).Add("b");

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
        var set = ImmutableHashMultiSet.CreateRange(new[] { "a", "a", "b" });
        Assert.AreEqual(2, set.CountOf("a"));
        Assert.AreEqual(1, set.CountOf("b"));
        Assert.AreEqual(0, set.CountOf("c"));

        Assert.AreEqual(2, set["a"]);
        Assert.AreEqual(1, set["b"]);
        Assert.ThrowsExactly<KeyNotFoundException>(() => set["c"]);
    }

    [TestMethod]
    public void Contains()
    {
        var set = ImmutableHashMultiSet.CreateRange(new[] { "a", "a", "b" });

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
}
