using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using TCD = NUnit.Framework.TestCaseData;
using System;
using System.Runtime.CompilerServices;
using Nemesis.Essentials.Design;
using System.Runtime.InteropServices;

namespace Tests
{
    [TestFixture]
    public class SpanBinaryReaderTests
    {
        [TestCase(-3)]
        [TestCase(-4)]
        public void Seek_ShouldThrowForInvalidOffset(int offset)
        {
            Assert.That(() =>
            {
                var sut = new SpanBinaryReader(new byte[] { 1, 2, 3, 4, 5 }, 2);

                sut.Seek(offset);
            }, Throws.InstanceOf<ArgumentOutOfRangeException>().And.Message.StartWith("After advancing by offset parameter, position should point to non-negative number"));
        }


        [TestCase(1, "3")]
        [TestCase(2, "3|4")]
        [TestCase(3, "3|4|5")]
        public void ReadExactly_ShouldBeAbleToReadBytes(int numBytes, string expectedArray)
        {
            var sut = new SpanBinaryReader(new byte[] { 1, 2, 3, 4, 5 }, 2);

            var actual = sut.ReadExactly(numBytes);
            var actualText = string.Join("|", actual.ToArray());

            Assert.That(actualText, Is.EqualTo(expectedArray));
        }

        [TestCase(0, 0, "")]
        [TestCase(1, 1, "3")]
        [TestCase(2, 2, "3|4")]
        [TestCase(3, 3, "3|4|5")]
        [TestCase(4, 3, "3|4|5|0")]
        [TestCase(5, 3, "3|4|5|0|0")]
        public void Read_ToBuffer_ShouldBeAbleToReadBytes(int bufferSize, int expectedCount, string expectedArray)
        {
            var sut = new SpanBinaryReader(new byte[] { 1, 2, 3, 4, 5 }, 2);
            //                                                ^

#if NET            
            Span<byte> temp = stackalloc byte[bufferSize];
            var buffer = MemoryMarshal.CreateSpan(ref MemoryMarshal.GetReference(temp), temp.Length);
#else
            Span<byte> buffer = new byte[bufferSize];
#endif


            var count = sut.ReadTo(buffer);
            var actualText = string.Join("|", buffer.ToArray());

            Assert.That(actualText, Is.EqualTo(expectedArray));
            Assert.That(count, Is.EqualTo(expectedCount));
        }

        [TestCase(0, "")]
        [TestCase(1, "3")]
        [TestCase(2, "3|4")]
        [TestCase(3, "3|4|5")]
        [TestCase(4, "3|4|5")]
        [TestCase(5, "3|4|5")]
        public void Read_NumBytes_ShouldBeAbleToReadBytes(int numBytes, string expectedArray)
        {
            var sut = new SpanBinaryReader(new byte[] { 1, 2, 3, 4, 5 }, 2);

            var actual = sut.Read(numBytes);
            var actualText = string.Join("|", actual.ToArray());

            Assert.That(actualText, Is.EqualTo(expectedArray));
        }

        [Test]
        public void ReadExactly_ShouldThrowWhenNotEnoughBytes()
        {
            Assert.That(() =>
            {
                var sut = new SpanBinaryReader(new byte[] { 1, 2, 3, 4, 5 }, 2);

                sut.ReadExactly(4);
            }, Throws.InstanceOf<ArgumentOutOfRangeException>().And.Message.StartWith("Not enough data to read 4 bytes from underlying buffer"));
        }

        [Test]
        public void IsEnd_ReturnsTrueWhenEndIsReached()
        {
            var sut = new SpanBinaryReader(new byte[] { 1, 2, 3, 4, 5 });
            Assert.That(sut.Length, Is.EqualTo(5));
            for (int i = 0; i < sut.Length; i++)
            {
                Assert.That(sut.IsEnd, Is.False);
                sut.Seek(1);
            }
            Assert.That(sut.IsEnd, Is.True);
        }


        delegate TResult ReaderDelegate<out TResult>(ref SpanBinaryReader reader);

        private static void ReadNumberHelper<T>(Func<int, T> generator, ReaderDelegate<T> readFunc, int count = 7) where T : struct
        {
            var source = new List<T>();
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);
            foreach (var number in Enumerable.Range(1, count).Select(generator))
            {
                source.Add(number);
                switch (number)
                {
                    case bool bl: bw.Write(bl); break;
                    case byte b: bw.Write(b); break;
                    case short s: bw.Write(s); break;
                    case int i: bw.Write(i); break;
                    case long l: bw.Write(l); break;
                    case float f: bw.Write(f); break;
                    case double d: bw.Write(d); break;
                    case decimal m: bw.Write(m); break;
                    default: throw new NotSupportedException();
                }
            }
            bw.Flush();


            var sut = new SpanBinaryReader(ms.ToArray());

            var size = Unsafe.SizeOf<T>();
            Assert.That(sut.Length, Is.EqualTo(source.Count * size));


            for (int i = 0; i < source.Count; i++)
                Assert.That(readFunc(ref sut), Is.EqualTo(source[i]), "Error at " + i);

            Assert.That(sut.IsEnd, Is.True);
        }

        [Test]
        public void ReadBoolean_ReturnsAppropriateValues() => ReadNumberHelper(i => i % 2 == 0, (ref SpanBinaryReader r) => r.ReadBoolean());

        [Test]
        public void ReadByte_ReturnsAppropriateValues() => ReadNumberHelper(i => (byte)(i * 11), (ref SpanBinaryReader r) => (byte)r.ReadByte());

        [Test]
        public void ReadInt16_ReturnsAppropriateValues() => ReadNumberHelper(i => (short)(i * 111), (ref SpanBinaryReader r) => r.ReadInt16());

        [Test]
        public void ReadInt32_ReturnsAppropriateValues() => ReadNumberHelper(i => i * 11111, (ref SpanBinaryReader r) => r.ReadInt32());

        [Test]
        public void ReadInt64_ReturnsAppropriateValues() => ReadNumberHelper(i => i * 1111111111111L, (ref SpanBinaryReader r) => r.ReadInt64());

#if NET
        [Test]
        public void ReadSingle_ReturnsAppropriateValues() => ReadNumberHelper(i => i * 3.14f, (ref SpanBinaryReader r) => r.ReadSingle());

        [Test]
        public void ReadDouble_ReturnsAppropriateValues() => ReadNumberHelper(i => i * 3.14, (ref SpanBinaryReader r) => r.ReadDouble());
#endif

        [Test]
        public void ReadDecimal_ReturnsAppropriateValues() => ReadNumberHelper(i => (decimal)Math.Pow(-100, i) * i * 3.14m + 0.123456789m, (ref SpanBinaryReader r) => r.ReadDecimal());



        private static IEnumerable<TCD> UnsignedVarintExamples = new (uint Number, byte[] Bytes)[]
        {
            (            0, new[]{ (byte)0b_00000000} ),
            (          127, new[]{ (byte)0b_01111111} ),
            (          128, new[]{ (byte)0b_10000000, (byte)0b_00000001} ),
            (          300, new[]{ (byte)0b_10101100, (byte)0b_00000010} ),
            (         8192, new[]{ (byte)0b_10000000, (byte)0b_01000000} ),
            (        16383, new[]{ (byte)0b_11111111, (byte)0b_01111111} ),
            (        16384, new[]{ (byte)0b_10000000, (byte)0b_10000000, (byte)0b_00000001} ),
            (      2097151, new[]{ (byte)0b_11111111, (byte)0b_11111111, (byte)0b_01111111} ),
            (      2097152, new[]{ (byte)0b_10000000, (byte)0b_10000000, (byte)0b_10000000, (byte)0b_00000001 }),
            (    134217728, new[]{ (byte)0b_10000000, (byte)0b_10000000, (byte)0b_10000000, (byte)0b_01000000 }),
            (    268435455, new[]{ (byte)0b_11111111, (byte)0b_11111111, (byte)0b_11111111, (byte)0b_01111111 }),

            (4294967295u-1, new[]{ (byte)0b_11111110, (byte)0b_11111111, (byte)0b_11111111, (byte)0b_11111111, (byte)0b_00001111 }),
            (  4294967295u, new[]{ (byte)0b_11111111, (byte)0b_11111111, (byte)0b_11111111, (byte)0b_11111111, (byte)0b_00001111 }),
        }.Select(p => new TCD(p.Bytes, p.Number).SetName($"UnsignedVarint_{p.Number:000000000}"));

        [TestCaseSource(nameof(UnsignedVarintExamples))]
        public void ReadUnsignedVarint_ReturnsAppropriateValues(byte[] bytes, uint expectedNumber)
        {
            /*static void WriteUnsignedVarint(uint value, Stream stream)
            {
                while ((value & 0xffffff80) != 0L)
                {
                    byte b = (byte)((value & 0x7f) | 0x80);
                    stream.WriteByte(b);
                    value >>= 7;
                }
                stream.WriteByte((byte)value);
            }
            using var ms2 = new MemoryStream();
            WriteUnsignedVarint(expectedNumber, ms2);
            foreach (var b in ms2.ToArray())
                Console.Write($"(byte)0b_{int.Parse(Convert.ToString(b, 2)):00000000}, ");*/


            using var ms = new MemoryStream();
            foreach (var b in bytes)
                ms.WriteByte(b);
            var buffer = ms.ToArray();
            var sut = new SpanBinaryReader(buffer);

            Assert.That(sut.ReadUnsignedVarint(), Is.EqualTo(expectedNumber));


            ms.Position = 0;
            using var br = new BinaryReader(ms);
            var sys = br.Read7BitEncodedInt64();
            Assert.That(sys, Is.EqualTo(expectedNumber), $"System read. Expected {expectedNumber} but got {sys}");


            static string SpanToString(ReadOnlySpan<byte> span) =>
                string.Join(", ", span.ToArray().Select(b => b.ToString("X2")));

            Assert.That(sut.IsEnd, Is.True, $"End not reached. Remaining [{SpanToString(sut.Remaining())}] out of [{SpanToString(buffer)}]");
        }
    }

    internal static class StreamHelpers
    {
        public static long Read7BitEncodedInt64(this BinaryReader reader)
        {
            ulong result = 0;
            byte byteReadJustNow;

            const int MaxBytesWithoutOverflow = 9;
            for (int shift = 0; shift < MaxBytesWithoutOverflow * 7; shift += 7)
            {
                // ReadByte handles end of stream cases for us.
                byteReadJustNow = reader.ReadByte();
                result |= (byteReadJustNow & 0x7Ful) << shift;

                if (byteReadJustNow <= 0x7Fu)
                    return (long)result;
            }

            byteReadJustNow = reader.ReadByte();
            if (byteReadJustNow > 0b_1u)
                throw new FormatException("Format_Bad7BitInt");

            result |= (ulong)byteReadJustNow << (MaxBytesWithoutOverflow * 7);
            return (long)result;
        }
    }

    internal static class SpanBufferReaderExtensions
    {
        /// <summary>
        /// Read an integer stored in variable-length format using signed decoding from <a href="http://code.google.com/apis/protocolbuffers/docs/encoding.html"> Google Protocol Buffers</a>
        /// </summary>
        /// <returns>The integer read</returns>
        /// <exception cref="OverflowException">Thrown if variable-length value does not terminate after 5 bytes have been read</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadVarint(this ref SpanBinaryReader reader)
        {
            var value = ReadUnsignedVarint(ref reader);
            return (int)((value >> 1) ^ -(value & 1));
        }

        /// <summary>
        /// Read an integer stored in variable-length format using unsigned decoding from <a href="http://code.google.com/apis/protocolbuffers/docs/encoding.html"> Google Protocol Buffers</a>
        /// </summary>
        /// /// <returns>The integer read</returns>
        /// <remarks>
        ///     Inspired by: https://github.com/apache/kafka/blob/2.5/clients/src/main/java/org/apache/kafka/common/utils/ByteUtils.java#L142
        /// </remarks>
        /// <exception cref="OverflowException">Thrown if variable-length value does not terminate after 5 bytes have been read</exception>
        public static uint ReadUnsignedVarint(this ref SpanBinaryReader reader)
        {
            int value = 0;
            int i = 0;
            int b;
            while (true)
            {
                b = reader.ReadByte();
                if (b == -1) throw new InvalidOperationException("Unexpected end of stream reading varint.");
                if ((b & 0x80) == 0) { break; }
                value |= (b & 0x7f) << i;
                i += 7;
                if (i > 28)
                    throw new OverflowException("Encoded varint is larger than uint.MaxValue");
            }
            value |= b << i;
            return (uint)value;
        }
    }
}