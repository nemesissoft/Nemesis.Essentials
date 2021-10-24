using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Nemesis.Essentials.Design
{
    public ref struct SpanBufferReader
    {
        private readonly ReadOnlySpan<byte> _buffer;
        private int _position;
        public int Length => _buffer.Length;

        public SpanBufferReader(ReadOnlySpan<byte> buffer, int position = 0)
        {
            _buffer = buffer;
            _position = position;
        }

        public void Reset() => _position = 0;
        public void AdvanceBy(int offset) => _position += offset;
        public bool IsEnd => _position >= _buffer.Length;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadByte() => _position >= _buffer.Length ? -1 : _buffer[_position++];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadInt32() => BinaryPrimitives.ReadInt32LittleEndian(BufferRead(4));
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long ReadInt64() => BinaryPrimitives.ReadInt64LittleEndian(BufferRead(8));
          

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> BufferRead(int numBytes)
        {
            Debug.Assert(numBytes is >= 2 and <= 16, "value of 1 should use ReadByte. For value > 16 implement more efficient read method");
            int origPos = _position;
            int newPos = origPos + numBytes;

            if ((uint)newPos > (uint)_buffer.Length)
            {
                _position = _buffer.Length;
                throw new ArgumentOutOfRangeException(nameof(numBytes), $"Not enough data to read {numBytes} bytes from underlying buffer");
            }

            var span = _buffer.Slice(origPos, numBytes);
            _position = newPos;
            return span;
        }

        public ReadOnlySpan<byte> Tail() => _buffer.Slice(_position, _buffer.Length - _position);
    }
}
