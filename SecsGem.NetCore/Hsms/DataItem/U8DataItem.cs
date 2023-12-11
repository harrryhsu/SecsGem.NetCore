using SecsGem.NetCore.Buffer;

namespace SecsGem.NetCore.Hsms
{
    public class U8DataItem : DataItem<ulong>
    {
        public override DataType Type => DataType.U8;

        protected override int DataBlockSize => 8;

        public U8DataItem(params ulong[] values)
        {
            Values = values.ToList();
        }

        public U8DataItem(DataItemConfig config)
        {
            for (var i = 0; i < config.Count; i++)
            {
                Values.Add(config.Buffer.ReadU8());
            }
        }

        protected override void WriteEachData(ByteBufferWriter buffer, ulong item)
        {
            buffer.Write(item);
        }
    }
}