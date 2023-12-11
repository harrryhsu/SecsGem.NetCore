using SecsGem.NetCore.Buffer;

namespace SecsGem.NetCore.Hsms
{
    public class ListDataItem : DataItem<DataItem>
    {
        public override DataType Type => DataType.List;

        protected override int DataBlockSize => 1;

        protected override int DataSize { get => Values.Sum(x => x.TotalSize); }

        public ListDataItem(params DataItem[] items)
        {
            Values = items.ToList();
        }

        public ListDataItem(DataItemConfig config)
        {
            for (var i = 0; i < config.Count; i++)
            {
                Values.Add(Create(config.Buffer));
            }
        }

        public override void Write(ByteBufferWriter buffer)
        {
            int lenByte = (Count <= 255) ? 1 : (Count <= 65535) ? 2 : 3;
            byte fb = (byte)((byte)Type | lenByte);
            buffer.Write(fb);

            for (var i = 0; i < lenByte; i++)
            {
                buffer.Write((byte)(Count << (8 * i)));
            }

            WriteData(buffer);
        }

        protected override void WriteEachData(ByteBufferWriter buffer, DataItem item)
        {
            item.Write(buffer);
        }
    }
}