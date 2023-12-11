using SecsGem.NetCore.Buffer;

namespace SecsGem.NetCore.Hsms
{
    public class U4DataItem : DataItem<uint>
    {
        public override DataType Type => DataType.U4;

        protected override int DataBlockSize => 4;

        public U4DataItem(params uint[] values)
        {
            Values = values.ToList();
        }

        public U4DataItem(DataItemConfig config)
        {
            for (var i = 0; i < config.Count; i++)
            {
                Values.Add(config.Buffer.ReadU4());
            }
        }

        protected override void WriteEachData(ByteBufferWriter buffer, uint item)
        {
            buffer.Write(item);
        }
    }
}