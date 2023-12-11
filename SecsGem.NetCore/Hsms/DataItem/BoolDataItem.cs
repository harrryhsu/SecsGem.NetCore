using SecsGem.NetCore.Buffer;

namespace SecsGem.NetCore.Hsms
{
    public class BoolDataItem : DataItem<bool>
    {
        public override DataType Type => DataType.Bool;

        protected override int DataBlockSize => 1;

        public BoolDataItem(params bool[] values)
        {
            Values = values.ToList();
        }

        public BoolDataItem(DataItemConfig config)
        {
            for (var i = 0; i < config.Count; i++)
            {
                Values.Add(config.Buffer.ReadBool());
            }
        }

        protected override void WriteEachData(ByteBufferWriter buffer, bool item)
        {
            buffer.Write(item);
        }
    }
}