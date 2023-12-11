using SecsGem.NetCore.Buffer;

namespace SecsGem.NetCore.Hsms
{
    public class I1DataItem : DataItem<byte>
    {
        public override DataType Type => DataType.I1;

        protected override int DataBlockSize => 1;

        public I1DataItem(params byte[] values)
        {
            Values = values.ToList();
        }

        public I1DataItem(DataItemConfig config)
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