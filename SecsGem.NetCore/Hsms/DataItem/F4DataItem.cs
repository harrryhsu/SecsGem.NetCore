using SecsGem.NetCore.Buffer;

namespace SecsGem.NetCore.Hsms
{
    public class F4DataItem : DataItem<float>
    {
        public override DataType Type => DataType.F4;

        protected override int DataBlockSize => 4;

        public F4DataItem(params float[] values)
        {
            Values = values.ToList();
        }

        public F4DataItem(DataItemConfig config)
        {
            for (var i = 0; i < config.Count; i++)
            {
                Values.Add(config.Buffer.ReadF4());
            }
        }

        protected override void WriteEachData(ByteBufferWriter buffer, float item)
        {
            buffer.Write(item);
        }
    }
}