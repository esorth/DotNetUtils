// .Net Utils - Misc utility classes/functions for use in .Net libraries.
// Written in 2024 by Eric Orth
//
// To the extent possible under law, the author(s) have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see http://creativecommons.org/publicdomain/zero/1.0/.

namespace DotNetUtils.Tests
{
    [TestClass()]
    public class ByteEnumeratorExtensionsTests
    {
        [TestMethod()]
        public void ReadByte()
        {
            Assert.AreEqual(
                1, (new byte[] { 1 } as IEnumerable<byte>).GetEnumerator().ReadByte());
            Assert.AreEqual(
                1, (new byte[] { 1, 2 } as IEnumerable<byte>).GetEnumerator().ReadByte());
        }

        [TestMethod()]
        public void ReadBytes()
        {
            CollectionAssert.AreEqual(
                new byte[] { 1, 2, 3, 4 },
                (new byte[] { 1, 2, 3, 4 } as IEnumerable<byte>).GetEnumerator().ReadBytes(4).ToArray());
            CollectionAssert.AreEqual(
                new byte[] { 1, 2, 3, 4 },
                (new byte[] { 1, 2, 3, 4, 5, 6 } as IEnumerable<byte>).GetEnumerator().ReadBytes(4).ToArray());
            CollectionAssert.AreEqual(
                Array.Empty<byte>(),
                (new byte[] { 1, 2, 3, 4, 5, 6 } as IEnumerable<byte>).GetEnumerator().ReadBytes(0).ToArray());
        }

        [TestMethod()]
        public void ReadLittleEndianUInt16()
        {
            Assert.AreEqual(13330, (new byte[] { 0x12, 0x34 } as IEnumerable<byte>)
                                       .GetEnumerator().ReadLittleEndianUInt16());
            Assert.AreEqual(13330, (new byte[] { 0x12, 0x34, 0x56 } as IEnumerable<byte>)
                                       .GetEnumerator().ReadLittleEndianUInt16());
            Assert.AreEqual(56574, (new byte[] { 0xFE, 0xDC } as IEnumerable<byte>)
                                       .GetEnumerator().ReadLittleEndianUInt16());
            Assert.AreEqual(56574, (new byte[] { 0xFE, 0xDC, 0xBA } as IEnumerable<byte>)
                                       .GetEnumerator().ReadLittleEndianUInt16());
        }

        [TestMethod()]
        public void ReadBigEndianUInt16()
        {
            Assert.AreEqual(4660, (new byte[] { 0x12, 0x34 } as IEnumerable<byte>)
                                       .GetEnumerator().ReadBigEndianUInt16());
            Assert.AreEqual(4660, (new byte[] { 0x12, 0x34, 0x56 } as IEnumerable<byte>)
                                       .GetEnumerator().ReadBigEndianUInt16());
            Assert.AreEqual(65244, (new byte[] { 0xFE, 0xDC } as IEnumerable<byte>)
                                       .GetEnumerator().ReadBigEndianUInt16());
            Assert.AreEqual(65244, (new byte[] { 0xFE, 0xDC, 0xBA } as IEnumerable<byte>)
                                       .GetEnumerator().ReadBigEndianUInt16());
        }

        [TestMethod()]
        public void ReadLittleEndianInt16()
        {
            Assert.AreEqual(13330, (new byte[] { 0x12, 0x34 } as IEnumerable<byte>)
                                       .GetEnumerator().ReadLittleEndianInt16());
            Assert.AreEqual(13330, (new byte[] { 0x12, 0x34, 0x56 } as IEnumerable<byte>)
                                       .GetEnumerator().ReadLittleEndianInt16());
            Assert.AreEqual(-8962, (new byte[] { 0xFE, 0xDC } as IEnumerable<byte>)
                                       .GetEnumerator().ReadLittleEndianInt16());
            Assert.AreEqual(-8962, (new byte[] { 0xFE, 0xDC, 0xBA } as IEnumerable<byte>)
                                       .GetEnumerator().ReadLittleEndianInt16());
        }

        [TestMethod()]
        public void ReadBigEndianInt16()
        {
            Assert.AreEqual(4660, (new byte[] { 0x12, 0x34 } as IEnumerable<byte>)
                                       .GetEnumerator().ReadBigEndianInt16());
            Assert.AreEqual(4660, (new byte[] { 0x12, 0x34, 0x56 } as IEnumerable<byte>)
                                       .GetEnumerator().ReadBigEndianInt16());
            Assert.AreEqual(-292, (new byte[] { 0xFE, 0xDC } as IEnumerable<byte>)
                                       .GetEnumerator().ReadBigEndianInt16());
            Assert.AreEqual(-292, (new byte[] { 0xFE, 0xDC, 0xBA } as IEnumerable<byte>)
                                       .GetEnumerator().ReadBigEndianInt16());
        }

        [TestMethod()]
        public void ReadLittleEndianUInt32()
        {
            Assert.AreEqual(2018915346u, (new byte[] { 0x12, 0x34, 0x56, 0x78 } as IEnumerable<byte>)
                                       .GetEnumerator().ReadLittleEndianUInt32());
            Assert.AreEqual(2018915346u, (new byte[] { 0x12, 0x34, 0x56, 0x78, 0x9A } as IEnumerable<byte>)
                                       .GetEnumerator().ReadLittleEndianUInt32());
            Assert.AreEqual(2562383102u, (new byte[] { 0xFE, 0xDC, 0xBA, 0x98 } as IEnumerable<byte>)
                                       .GetEnumerator().ReadLittleEndianUInt32());
            Assert.AreEqual(2562383102u, (new byte[] { 0xFE, 0xDC, 0xBA, 0x98, 0x76 } as IEnumerable<byte>)
                                       .GetEnumerator().ReadLittleEndianUInt32());
        }

        [TestMethod()]
        public void ReadBigEndianUInt32()
        {
            Assert.AreEqual(305419896u, (new byte[] { 0x12, 0x34, 0x56, 0x78 } as IEnumerable<byte>)
                                       .GetEnumerator().ReadBigEndianUInt32());
            Assert.AreEqual(305419896u, (new byte[] { 0x12, 0x34, 0x56, 0x78, 0x9A } as IEnumerable<byte>)
                                       .GetEnumerator().ReadBigEndianUInt32());
            Assert.AreEqual(4275878552u, (new byte[] { 0xFE, 0xDC, 0xBA, 0x98 } as IEnumerable<byte>)
                                       .GetEnumerator().ReadBigEndianUInt32());
            Assert.AreEqual(4275878552u, (new byte[] { 0xFE, 0xDC, 0xBA, 0x98, 0x76 } as IEnumerable<byte>)
                                       .GetEnumerator().ReadBigEndianUInt32());
        }

        [TestMethod()]
        public void ReadLittleEndianInt32()
        {
            Assert.AreEqual(2018915346, (new byte[] { 0x12, 0x34, 0x56, 0x78 } as IEnumerable<byte>)
                                       .GetEnumerator().ReadLittleEndianInt32());
            Assert.AreEqual(2018915346, (new byte[] { 0x12, 0x34, 0x56, 0x78, 0x9A } as IEnumerable<byte>)
                                       .GetEnumerator().ReadLittleEndianInt32());
            Assert.AreEqual(-1732584194, (new byte[] { 0xFE, 0xDC, 0xBA, 0x98 } as IEnumerable<byte>)
                                       .GetEnumerator().ReadLittleEndianInt32());
            Assert.AreEqual(-1732584194, (new byte[] { 0xFE, 0xDC, 0xBA, 0x98, 0x76 } as IEnumerable<byte>)
                                       .GetEnumerator().ReadLittleEndianInt32());
        }

        [TestMethod()]
        public void ReadBigEndianInt32()
        {
            Assert.AreEqual(305419896, (new byte[] { 0x12, 0x34, 0x56, 0x78 } as IEnumerable<byte>)
                                       .GetEnumerator().ReadBigEndianInt32());
            Assert.AreEqual(305419896, (new byte[] { 0x12, 0x34, 0x56, 0x78, 0x9A } as IEnumerable<byte>)
                                       .GetEnumerator().ReadBigEndianInt32());
            Assert.AreEqual(-19088744, (new byte[] { 0xFE, 0xDC, 0xBA, 0x98 } as IEnumerable<byte>)
                                       .GetEnumerator().ReadBigEndianInt32());
            Assert.AreEqual(-19088744, (new byte[] { 0xFE, 0xDC, 0xBA, 0x98, 0x76 } as IEnumerable<byte>)
                                       .GetEnumerator().ReadBigEndianInt32());
        }
    }
}
