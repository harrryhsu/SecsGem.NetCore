using SecsGem.NetCore.Buffer;

namespace SecsGem.NetCore.Hsms
{
    public class I4DataItem : DataItem<int>
    {
        public override DataType Type => DataType.I4;

        protected override int DataBlockSize => 4;

        public I4DataItem(params int[] values)
        {
            Values = values.ToList();
        }

        public I4DataItem(DataItemConfig config)
        {
            for (var i = 0; i < config.Count; i++)
            {
                Values.Add(config.Buffer.ReadI4());
            }
        }

        protected override void WriteEachData(ByteBufferWriter buffer, int item)
        {
            buffer.Write(item);
        }
    }
}