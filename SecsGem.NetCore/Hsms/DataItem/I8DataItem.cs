using SecsGem.NetCore.Buffer;

namespace SecsGem.NetCore.Hsms
{
    public class I8DataItem : DataItem<long>
    {
        public override DataType Type => DataType.I8;

        protected override int DataBlockSize => 8;

        public I8DataItem(params long[] values)
        {
            Values = values.ToList();
        }

        public I8DataItem(DataItemConfig config)
        {
            for (var i = 0; i < config.Count; i++)
            {
                Values.Add(config.Buffer.ReadI8());
            }
        }

        protected override void WriteEachData(ByteBufferWriter buffer, long item)
        {
            buffer.Write(item);
        }
    }
}