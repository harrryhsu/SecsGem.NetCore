using System.Buffers;
using System.Buffers.Binary;
using System.Text;

namespace SecsGem.NetCore.Buffer
{
    public class ByteBufferReader
    {
        private readonly MemoryStream _buffer;

        public ByteBufferReader(byte[] buffer)
        {
            _buffer = new(buffer);
        }

        public byte ReadByte()
        {
            return (byte)_buffer.ReadByte();
        }

        public bool ReadBool()
        {
            return ReadByte() != 0;
        }

        public byte[] ReadBytes(int count)
        {
            byte[] bytes = new byte[count];
            _buffer.Read(bytes);
            return bytes;
        }

        public string ReadString(int count)
        {
            return Encoding.UTF8.GetString(ReadBytes(count));
        }

        public short ReadI2()
        {
            var bytes = ReadBytes(2);
            var value = BitConverter.ToInt16(bytes);
            return BinaryPrimitives.ReverseEndianness(value);
        }

        public int ReadI4()
        {
            var bytes = ReadBytes(4);
            var value = BitConverter.ToInt32(bytes);
            return BinaryPrimitives.ReverseEndianness(value);
        }

        public long ReadI8()
        {
            var bytes = ReadBytes(8);
            var value = BitConverter.ToInt64(bytes);
            return BinaryPrimitives.ReverseEndianness(value);
        }

        public ushort ReadU2()
        {
            var bytes = ReadBytes(2);
            var value = BitConverter.ToUInt16(bytes);
            return BinaryPrimitives.ReverseEndianness(value);
        }

        public uint ReadU4()
        {
            var bytes = ReadBytes(4);
            var value = BitConverter.ToUInt32(bytes);
            return BinaryPrimitives.ReverseEndianness(value);
        }

        public ulong ReadU8()
        {
            var bytes = ReadBytes(8);
            var value = BitConverter.ToUInt64(bytes);
            return BinaryPrimitives.ReverseEndianness(value);
        }

        public float ReadF4()
        {
            var bytes = ReadBytes(4).Reverse().ToArray();
            return BitConverter.ToSingle(bytes);
        }

        public double ReadF8()
        {
            var bytes = ReadBytes(8).Reverse().ToArray();
            return BitConverter.ToDouble(bytes);
        }

        public static uint ReadU4(byte[] buffer)
        {
            var value = BitConverter.ToUInt32(buffer);
            return BinaryPrimitives.ReverseEndianness(value);
        }
    }
}