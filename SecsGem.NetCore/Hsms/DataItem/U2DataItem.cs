using SecsGem.NetCore.Buffer;

namespace SecsGem.NetCore.Hsms
{
    public class U2DataItem : DataItem<ushort>
    {
        public override DataType Type => DataType.U2;

        protected override int DataBlockSize => 2;

        public U2DataItem(params ushort[] values)
        {
            Values = values.ToList();
        }

        public U2DataItem(DataItemConfig config)
        {
            for (var i = 0; i < config.Count; i++)
            {
                Values.Add(config.Buffer.ReadU2());
            }
        }

        protected override void WriteEachData(ByteBufferWriter buffer, ushort item)
        {
            buffer.Write(item);
        }
    }
}