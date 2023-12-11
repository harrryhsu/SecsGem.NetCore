using SecsGem.NetCore.Buffer;

namespace SecsGem.NetCore.Hsms
{
    public class F8DataItem : DataItem<double>
    {
        public override DataType Type => DataType.F8;

        protected override int DataBlockSize => 8;

        public F8DataItem(params double[] values)
        {
            Values = values.ToList();
        }

        public F8DataItem(DataItemConfig config)
        {
            for (var i = 0; i < config.Count; i++)
            {
                Values.Add(config.Buffer.ReadF8());
            }
        }

        protected override void WriteEachData(ByteBufferWriter buffer, double item)
        {
            buffer.Write(item);
        }
    }
}