using SecsGem.NetCore.Buffer;

namespace SecsGem.NetCore.Hsms
{
    public enum DataType
    {
        List = 0,    // List

        Bin = 32,    // Binary

        Bool = 36,   // Boolean

        A = 64,      // ASCII

        I8 = 96,     // 8 byte integer (signed)

        I4 = 112,    // 4 byte integer (signed)

        I2 = 104,    // 2 byte integer (signed)

        I1 = 100,    // 1 byte integer (signed)

        F8 = 128,    // 8 byte floating point

        F4 = 144,    // 4 byte floating point

        U8 = 160,    // 8 byte integer

        U4 = 176,    // 4 byte integer

        U2 = 168,    // 2 byte integer

        U1 = 164,    // 1 byte integer
    }

    public abstract class DataItem : IHsmsWritable
    {
        public const byte MASK_KILL_NO_LEN_BYTES = 252;

        public const byte MASK_NO_LEN_BYTES = 3;

        public abstract DataType Type { get; }

        protected abstract int DataSize { get; }

        public virtual int TotalSize { get => DataSize + GetDataHeaderLength(); }

        public abstract int Count { get; }

        public TItem Cast<TItem>() where TItem : DataItem
        {
            return this as TItem ?? throw new HsmsException("Not Matching Value Type");
        }

        public int GetDataHeaderLength()
        {
            int lenByte = (Count <= 255) ? 1 : (Count <= 65535) ? 2 : 3;
            return lenByte + 1;
        }

        public virtual void Write(ByteBufferWriter buffer)
        {
            int lenByte = (Count <= 255) ? 1 : (Count <= 65535) ? 2 : 3;
            byte fb = (byte)((byte)Type | lenByte);

            buffer.Write(fb);

            for (var i = 0; i < lenByte; i++)
            {
                buffer.Write((byte)(DataSize << (8 * i)));
            }

            WriteData(buffer);
        }

        protected abstract void WriteData(ByteBufferWriter buffer);

        public static DataItem Create(ByteBufferReader buffer)
        {
            var fb = buffer.ReadByte();
            DataType type = (DataType)(fb & MASK_KILL_NO_LEN_BYTES);
            byte lenByte = (byte)(fb & MASK_NO_LEN_BYTES);

            int size = 0, count = 0;
            for (var i = 0; i < lenByte; i++)
            {
                size += (byte)(buffer.ReadByte() << (8 * (lenByte - i - 1)));
            }

            var config = new DataItemConfig
            {
                Buffer = buffer,
                DataSize = size,
                TotalSize = 1 + lenByte + size
            };

            switch (type)
            {
                case DataType.List:
                case DataType.A:
                case DataType.Bool:
                case DataType.Bin:
                case DataType.I1:
                case DataType.U1:
                    count = size;
                    break;

                case DataType.I2:
                case DataType.U2:
                    count = size / 2;
                    break;

                case DataType.I4:
                case DataType.U4:
                case DataType.F4:
                    count = size / 4;
                    break;

                case DataType.I8:
                case DataType.U8:
                case DataType.F8:
                    count = size / 8;
                    break;
            }

            config.Count = count;

            switch (type)
            {
                case DataType.List:
                    return new ListDataItem(config);

                case DataType.A:
                    return new ADataItem(config);

                case DataType.Bool:
                    return new BoolDataItem(config);

                case DataType.Bin:
                    return new BinDataItem(config);

                case DataType.I1:
                    return new I1DataItem(config);

                case DataType.U1:
                    return new U1DataItem(config);

                case DataType.I2:
                    return new I2DataItem(config);

                case DataType.U2:
                    return new U2DataItem(config);

                case DataType.I4:
                    return new I4DataItem(config);

                case DataType.U4:
                    return new U4DataItem(config);

                case DataType.F4:
                    return new F4DataItem(config);

                case DataType.I8:
                    return new I8DataItem(config);

                case DataType.U8:
                    return new U8DataItem(config);

                case DataType.F8:
                    return new F8DataItem(config);

                default:
                    throw new HsmsException($"Unknown Data Type: {type}");
            }
        }

        public DataItem this[int key]
        {
            get
            {
                var list = Cast<ListDataItem>();
                if (key >= list.Values.Count) throw new HsmsException("Child index out of range");
                return list.Values[key];
            }
        }

        public List<DataItem> GetListItem()
        {
            var item = Cast<ListDataItem>();
            return item.Values;
        }

        public string GetString()
        {
            var item = Cast<ADataItem>();
            return item.Value;
        }

        public byte GetBin()
        {
            var item = Cast<BinDataItem>();
            return item.Values[0];
        }

        public byte[] GetBins()
        {
            var item = Cast<BinDataItem>();
            return item.Values.ToArray();
        }

        public bool GetBool(int index = 0)
        {
            var item = Cast<BoolDataItem>();
            return item.Values[index];
        }

        public float GetF4(int index = 0)
        {
            var item = Cast<F4DataItem>();
            return item.Values[index];
        }

        public double GetF8(int index = 0)
        {
            var item = Cast<F8DataItem>();
            return item.Values[index];
        }

        public byte GetI1(int index = 0)
        {
            var item = Cast<I1DataItem>();
            return item.Values[index];
        }

        public short GetI2(int index = 0)
        {
            var item = Cast<I2DataItem>();
            return item.Values[index];
        }

        public int GetI4(int index = 0)
        {
            var item = Cast<I4DataItem>();
            return item.Values[index];
        }

        public long GetI8(int index = 0)
        {
            var item = Cast<I8DataItem>();
            return item.Values[index];
        }

        public byte GetU1(int index = 0)
        {
            var item = Cast<U1DataItem>();
            return item.Values[index];
        }

        public ushort GetU2(int index = 0)
        {
            var item = Cast<U2DataItem>();
            return item.Values[index];
        }

        public uint GetU4(int index = 0)
        {
            var item = Cast<U4DataItem>();
            return item.Values[index];
        }

        public ulong GetU8(int index = 0)
        {
            var item = Cast<U8DataItem>();
            return item.Values[index];
        }
    }

    public abstract class DataItem<TType> : DataItem
    {
        public List<TType> Values { get; set; } = new();

        public override int Count { get => Values.Count; }

        protected override int DataSize { get => Count * DataBlockSize; }

        protected abstract int DataBlockSize { get; }

        protected override void WriteData(ByteBufferWriter buffer)
        {
            foreach (var item in Values)
            {
                WriteEachData(buffer, item);
            }
        }

        protected abstract void WriteEachData(ByteBufferWriter buffer, TType item);
    }

    public class DataItemConfig
    {
        public ByteBufferReader Buffer { get; set; }

        public int Count { get; set; }

        public int DataSize { get; set; }

        public int TotalSize { get; set; }
    }
}