using SecsGem.NetCore.Buffer;

namespace SecsGem.NetCore.Hsms
{
    public class ADataItem : DataItem
    {
        public override DataType Type => DataType.A;

        public string Value { get; set; } = "";

        public override int Count { get => Value?.Length ?? 0; }

        protected override int DataSize => Value?.Length ?? 0;

        public ADataItem(string value)
        {
            Value = value;
        }

        public ADataItem(DataItemConfig config)
        {
            Value = config.Buffer.ReadString(config.Count);
        }

        protected override void WriteData(ByteBufferWriter buffer)
        {
            buffer.Write(Value);
        }
    }
}