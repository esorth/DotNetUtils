using System.Collections.Generic;
using System.Security.Cryptography;

namespace DotNetUtils.Tests
{
    [TestClass()]
    public class TreeIndexedDictionaryTests
    {
        [TestMethod()]
        public void RandomOrderAdditions()
        {
            for (int repeat = 0; repeat < 10; repeat++)
            {
                var randomList = Enumerable.Range(0, 100).ToList();
                for (int i = 0; i < 100; i++)
                {
                    int num = randomList[i];
                    int selection = RandomNumberGenerator.GetInt32(i, 100);
                    randomList[i] = randomList[selection];
                    randomList[selection] = num;
                }

                TreeIndexedDictionary<int, int> tree = new();
                for (int i = 0; i < 100; i++)
                {
                    tree.Add(randomList[i], i);

                    Assert.AreEqual(i + 1, tree.Count);
                    int last = -1;
                    foreach (KeyValuePair<int, int> item in tree)
                    {
                        Assert.AreEqual(item.Key, randomList[item.Value]);
                        Assert.IsTrue(tree.ContainsKey(item.Key));
                        Assert.AreEqual(tree.IndexOf(item.Key), tree.IndexOf(item));
                        Assert.IsGreaterThan(last, item.Key);
                        last = item.Key;
                    }
                    Assert.IsLessThan(100, last);
                }

                int j = 0;
                foreach (KeyValuePair<int, int> item in tree)
                {
                    Assert.AreEqual(j++, item.Key);
                    Assert.AreEqual(item.Key, randomList[item.Value]);
                    Assert.AreEqual(item.Key, tree.IndexOf(item.Key));
                    Assert.AreEqual(item.Key, tree.IndexOf(item));
                    Assert.AreEqual(item, tree.At(item.Key));
                    Assert.IsTrue(tree.ContainsKey(item.Key));
                    Assert.IsTrue(tree.Contains(item));
                    Assert.IsFalse(tree.Contains(new KeyValuePair<int, int>(item.Key, -1)));

                    TreeIndexedDictionary<int, int> singularSubtree = tree.At(item.Key..(item.Key + 1));
                    Assert.AreEqual(1, singularSubtree.Count);
                    Assert.AreEqual(item, singularSubtree.At(0));

                    Assert.AreEqual(item, tree.TryGetCeilingItem(item.Key));
                    Assert.AreEqual(item.Key, tree.TryGetIndexOfCeilingItem(item.Key));
                    Assert.AreEqual(item, tree.TryGetFloorItem(item.Key));
                    Assert.AreEqual(item.Key, tree.TryGetIndexOfFloorItem(item.Key));

                    KeyValuePair<int, int>? lower = tree.TryGetLowerItem(item.Key);
                    int? lowerIndex = tree.TryGetIndexOfLowerItem(item.Key);
                    if (item.Key == 0)
                    {
                        Assert.IsFalse(lower.HasValue);
                        Assert.IsFalse(lowerIndex.HasValue);
                    }
                    else
                    {
                        Assert.IsTrue(lower.HasValue);
                        Assert.AreEqual(item.Key - 1, lower.Value.Key);
                        Assert.IsTrue(lowerIndex.HasValue);
                        Assert.AreEqual(item.Key - 1, lowerIndex.Value);
                    }

                    KeyValuePair<int, int>? higher = tree.TryGetHigherItem(item.Key);
                    int? higherIndex = tree.TryGetIndexOfHigherItem(item.Key);
                    if (item.Key == tree.Count - 1)
                    {
                        Assert.IsFalse(higher.HasValue);
                        Assert.IsFalse(higherIndex.HasValue);
                    }
                    else
                    {
                        Assert.IsTrue(higher.HasValue);
                        Assert.AreEqual(item.Key + 1, higher.Value.Key);
                        Assert.IsTrue(higherIndex.HasValue);
                        Assert.AreEqual(item.Key + 1, higherIndex.Value);
                    }
                }

                TreeIndexedDictionary<int, int> emptySubtree = tree.At(5..5);
                Assert.AreEqual(0, emptySubtree.Count);

                TreeIndexedDictionary<int, int> completeSubtree = tree.At(..);
                CollectionAssert.AreEqual(tree, completeSubtree);

                Assert.IsFalse(tree.ContainsKey(-1));

                Assert.IsFalse(tree.TryGetLowerItem(-1).HasValue);
                Assert.IsFalse(tree.TryGetIndexOfLowerItem(-1).HasValue);
                Assert.IsFalse(tree.TryGetFloorItem(-1).HasValue);
                Assert.IsFalse(tree.TryGetIndexOfFloorItem(-1).HasValue);
                Assert.IsFalse(tree.TryGetCeilingItem(100).HasValue);
                Assert.IsFalse(tree.TryGetIndexOfCeilingItem(100).HasValue);
                Assert.IsFalse(tree.TryGetHigherItem(100).HasValue);
                Assert.IsFalse(tree.TryGetIndexOfHigherItem(100).HasValue);

                KeyValuePair<int, int>? lowestCeiling = tree.TryGetCeilingItem(-1);
                Assert.IsTrue(lowestCeiling.HasValue);
                Assert.AreEqual(0, lowestCeiling.Value.Key);

                int? lowestHigher = tree.TryGetIndexOfHigherItem(-1);
                Assert.IsTrue(lowestHigher.HasValue);
                Assert.AreEqual(0, lowestHigher.Value);

                int? highestFloor = tree.TryGetIndexOfFloorItem(100);
                Assert.IsTrue(highestFloor.HasValue);
                Assert.AreEqual(99, highestFloor.Value);

                int? highestLower = tree.TryGetIndexOfLowerItem(100);
                Assert.IsTrue(highestLower.HasValue);
                Assert.AreEqual(99, highestLower.Value);
            }
        }

        [TestMethod()]
        public void RandomOrderRemovals()
        {
            for (int repeat = 0; repeat < 10; repeat++)
            {
                var addList = Enumerable.Range(0, 100).ToList();
                var removeList = Enumerable.Range(0, 100).ToList();
                for (int i = 0; i < 100; i++)
                {
                    int num = addList[i];
                    int selection = RandomNumberGenerator.GetInt32(i, 100);
                    addList[i] = addList[selection];
                    addList[selection] = num;

                    num = removeList[i];
                    selection = RandomNumberGenerator.GetInt32(i, 100);
                    removeList[i] = removeList[selection];
                    removeList[selection] = num;
                }

                TreeIndexedDictionary<int, int> tree = new();
                for (int i = 0; i < 100; i++)
                {
                    tree.Add(addList[i], i);
                }

                for (int i = 0; i < 100; i++)
                {
                    Assert.IsTrue(tree.Remove(removeList[i]));
                    Assert.AreEqual(99 - i, tree.Count);

                    int j = 0;
                    int last = -1;
                    foreach (KeyValuePair<int, int> item in tree)
                    {
                        Assert.AreEqual(item.Key, addList[item.Value]);
                        Assert.IsTrue(tree.ContainsKey(item.Key));
                        Assert.AreEqual(j, tree.IndexOf(item));
                        Assert.AreEqual(item, tree.At(j++));
                        Assert.IsGreaterThan(last, item.Key);
                        last = item.Key;
                    }
                }
            }
        }

        [TestMethod()]
        public void IndexBasedRemovals()
        {
            for (int repeat = 0; repeat < 10; repeat++)
            {
                var addList = Enumerable.Range(0, 100).ToList();
                for (int i = 0; i < 100; i++)
                {
                    int num = addList[i];
                    int selection = RandomNumberGenerator.GetInt32(i, 100);
                    addList[i] = addList[selection];
                    addList[selection] = num;
                }

                TreeIndexedDictionary<int, int> tree = new();
                for (int i = 0; i < 100; i++)
                {
                    tree.Add(addList[i], i);
                }

                for (int i = 0; i < 100; i++)
                {
                    if (RandomNumberGenerator.GetInt32(2) == 0)
                    {
                        tree.RemoveAt(RandomNumberGenerator.GetInt32(tree.Count));
                    }
                    else
                    {
                        tree.RemoveAt(^RandomNumberGenerator.GetInt32(1, tree.Count + 1));

                    }
                    Assert.AreEqual(99 - i, tree.Count);

                    int j = 0;
                    int last = -1;
                    foreach (KeyValuePair<int, int> item in tree)
                    {
                        Assert.AreEqual(item.Key, addList[item.Value]);
                        Assert.IsTrue(tree.ContainsKey(item.Key));
                        Assert.AreEqual(j, tree.IndexOf(item));
                        Assert.AreEqual(item, tree.At(j++));
                        Assert.IsGreaterThan(last, item.Key);
                        last = item.Key;
                    }
                }
            }
        }
    }
}
