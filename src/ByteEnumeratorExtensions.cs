// .Net Utils - Misc utility classes/functions for use in .Net libraries.
// Written in 2024 by Eric Orth
//
// To the extent possible under law, the author(s) have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see http://creativecommons.org/publicdomain/zero/1.0/.

using System.Buffers.Binary;
using System.Collections.Immutable;

namespace DotNetUtils
{
    public static class ByteEnumeratorExtensions
    {
        public static byte ReadByte(this IEnumerator<byte> e)
        {
            if (e is null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            if (!e.MoveNext())
            {
                throw new ArgumentException("Not enough data.", nameof(e));
            }
            return e.Current;
        }

        public static IImmutableList<byte> ReadBytes(this IEnumerator<byte> e, int numBytes)
        {
            if (numBytes < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(numBytes), "Must be at least 0.");
            }

            ImmutableArray<byte>.Builder builder = ImmutableArray.CreateBuilder<byte>(numBytes);
            for (int i = 0; i < numBytes; i++)
            {
                builder.Add(e.ReadByte());
            }
            return builder.ToImmutable();
        }

        public static UInt16 ReadLittleEndianUInt16(this IEnumerator<byte> e)
        {
            IImmutableList<byte> bytes = e.ReadBytes(2);
            return BinaryPrimitives.ReadUInt16LittleEndian(bytes.ToImmutableSpan(..));
        }

        public static UInt16 ReadBigEndianUInt16(this IEnumerator<byte> e)
        {
            IImmutableList<byte> bytes = e.ReadBytes(2);
            return BinaryPrimitives.ReadUInt16BigEndian(bytes.ToImmutableSpan(..));
        }

        public static Int16 ReadLittleEndianInt16(this IEnumerator<byte> e)
        {
            IImmutableList<byte> bytes = e.ReadBytes(2);
            return BinaryPrimitives.ReadInt16LittleEndian(bytes.ToImmutableSpan(..));
        }

        public static Int16 ReadBigEndianInt16(this IEnumerator<byte> e)
        {
            IImmutableList<byte> bytes = e.ReadBytes(2);
            return BinaryPrimitives.ReadInt16BigEndian(bytes.ToImmutableSpan(..));
        }

        public static UInt32 ReadLittleEndianUInt32(this IEnumerator<byte> e)
        {
            IImmutableList<byte> bytes = e.ReadBytes(4);
            return BinaryPrimitives.ReadUInt32LittleEndian(bytes.ToImmutableSpan(..));
        }

        public static UInt32 ReadBigEndianUInt32(this IEnumerator<byte> e)
        {
            IImmutableList<byte> bytes = e.ReadBytes(4);
            return BinaryPrimitives.ReadUInt32BigEndian(bytes.ToImmutableSpan(..));
        }

        public static Int32 ReadLittleEndianInt32(this IEnumerator<byte> e)
        {
            IImmutableList<byte> bytes = e.ReadBytes(4);
            return BinaryPrimitives.ReadInt32LittleEndian(bytes.ToImmutableSpan(..));
        }

        public static Int32 ReadBigEndianInt32(this IEnumerator<byte> e)
        {
            IImmutableList<byte> bytes = e.ReadBytes(4);
            return BinaryPrimitives.ReadInt32BigEndian(bytes.ToImmutableSpan(..));
        }
    }
}
