using SecsGem.NetCore.Buffer;

namespace SecsGem.NetCore.Hsms
{
    public interface IHsmsWritable
    {
        public void Write(ByteBufferWriter buffer);
    }
}