using SecsGem.NetCore.Buffer;

namespace SecsGem.NetCore.Hsms
{
    public class U1DataItem : DataItem<byte>
    {
        public override DataType Type => DataType.U1;

        protected override int DataBlockSize => 1;

        public U1DataItem(params byte[] values)
        {
            Values = values.ToList();
        }

        public U1DataItem(DataItemConfig config)
        {
            for (var i = 0; i < config.Count; i++)
            {
                Values.Add(config.Buffer.ReadByte());
            }
        }

        protected override void WriteEachData(ByteBufferWriter buffer, byte item)
        {
            buffer.Write(item);
        }
    }
}