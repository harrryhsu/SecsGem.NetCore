using SecsGem.NetCore.Buffer;

namespace SecsGem.NetCore.Hsms
{
    public class I2DataItem : DataItem<short>
    {
        public override DataType Type => DataType.I2;

        protected override int DataBlockSize => 2;

        public I2DataItem(params short[] values)
        {
            Values = values.ToList();
        }

        public I2DataItem(DataItemConfig config)
        {
            for (var i = 0; i < config.Count; i++)
            {
                Values.Add(config.Buffer.ReadI2());
            }
        }

        protected override void WriteEachData(ByteBufferWriter buffer, short item)
        {
            buffer.Write(item);
        }
    }
}