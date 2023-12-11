using SecsGem.NetCore.Buffer;

namespace SecsGem.NetCore.Hsms
{
    public class BinDataItem : DataItem<byte>
    {
        public override DataType Type => DataType.Bin;

        protected override int DataBlockSize => 1;

        public BinDataItem(byte value)
        {
            Values.Add(value);
        }

        public BinDataItem(byte[] value)
        {
            Values = value.ToList();
        }

        public BinDataItem(DataItemConfig config)
        {
            Values = config.Buffer.ReadBytes(config.Count).ToList();
        }

        protected override void WriteData(ByteBufferWriter buffer)
        {
            buffer.Write(Values);
        }

        protected override void WriteEachData(ByteBufferWriter buffer, byte item)
        {
        }
    }
}