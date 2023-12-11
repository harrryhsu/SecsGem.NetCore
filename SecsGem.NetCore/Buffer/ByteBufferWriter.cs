using System.Buffers;
using System.Buffers.Binary;
using System.Text;

namespace SecsGem.NetCore.Buffer
{
    public class ByteBufferWriter
    {
        private readonly ArrayBufferWriter<byte> _buffer = new();

        public ByteBufferWriter()
        {
        }

        public ReadOnlyMemory<byte> ToMemory()
        {
            return _buffer.WrittenMemory;
        }

        public void Write(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value ?? string.Empty);
            _buffer.Write(bytes);
        }

        public void Write(byte value)
        {
            _buffer.Write(new byte[] { value });
        }

        public void Write(IEnumerable<byte> value)
        {
            _buffer.Write(value.ToArray());
        }

        public void Write(bool value)
        {
            var bytes = (byte)(value ? 0x1 : 0x0);
            Write(bytes);
        }

        public void Write(float value)
        {
            var bytes = BitConverter.GetBytes(value).Reverse().ToArray();
            _buffer.Write(bytes);
        }

        public void Write(double value)
        {
            var bytes = BitConverter.GetBytes(value).Reverse().ToArray();
            _buffer.Write(bytes);
        }

        public void Write(short value)
        {
            value = BinaryPrimitives.ReverseEndianness(value);
            var bytes = BitConverter.GetBytes(value);
            _buffer.Write(bytes);
        }

        public void Write(int value)
        {
            value = BinaryPrimitives.ReverseEndianness(value);
            var bytes = BitConverter.GetBytes(value);
            _buffer.Write(bytes);
        }

        public void Write(long value)
        {
            value = BinaryPrimitives.ReverseEndianness(value);
            var bytes = BitConverter.GetBytes(value);
            _buffer.Write(bytes);
        }

        public void Write(ushort value)
        {
            value = BinaryPrimitives.ReverseEndianness(value);
            var bytes = BitConverter.GetBytes(value);
            _buffer.Write(bytes);
        }

        public void Write(uint value)
        {
            value = BinaryPrimitives.ReverseEndianness(value);
            var bytes = BitConverter.GetBytes(value);
            _buffer.Write(bytes);
        }

        public void Write(ulong value)
        {
            value = BinaryPrimitives.ReverseEndianness(value);
            var bytes = BitConverter.GetBytes(value);
            _buffer.Write(bytes);
        }
    }
}