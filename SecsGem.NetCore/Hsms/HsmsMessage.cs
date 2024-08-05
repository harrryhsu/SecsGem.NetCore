using SecsGem.NetCore.Buffer;

namespace SecsGem.NetCore.Hsms
{
    public class HsmsMessage : IHsmsWritable
    {
        public static HsmsMessageBuilder Builder => new();

        public HsmsHeader Header { get; set; } = new();

        public DataItem Root { get; set; }

        public int? ReplyTimeout { get; set; }

        public HsmsMessage()
        {
        }

        public HsmsMessage(ByteBufferReader buffer)
        {
            Header = new HsmsHeader(buffer);
            if (Header.Size > 10)
                Root = DataItem.Create(buffer);
        }

        public void Write(ByteBufferWriter buffer)
        {
            Header.Size = (uint)(Root?.TotalSize ?? 0) + 10;
            Header.Write(buffer);
            Root?.Write(buffer);
        }

        public override string ToString()
        {
            return $"{Header.SType} Q{Header.Context >> 24}-S{Header.S}F{Header.F}";
        }
    }
}