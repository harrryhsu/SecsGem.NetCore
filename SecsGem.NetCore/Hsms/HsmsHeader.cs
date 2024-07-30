using SecsGem.NetCore.Buffer;

namespace SecsGem.NetCore.Hsms
{
    public enum HsmsMessageType
    {
        DataMessage = 0,

        SelectReq = 1,

        SelectRsp = 2,

        DeselectReq = 3,

        DeselectRsp = 4,

        LinkTestReq = 5,

        LinkTestRsp = 6,

        RejectReq = 7,

        SeparateReq = 9
    }

    public enum HsmsErrorCode
    {
        UnrecognizedDeviceId = 1,

        UnrecognizedStream = 3,

        UnrecognizedFunction = 5,

        IllegalData = 7,

        TransactionTimeout = 9,

        DataTooLong = 11,

        ConversionTimeout = 13,
    }

    public class HsmsHeader : IHsmsWritable
    {
        public uint Size { get; set; }

        public ushort Device { get; set; }

        public byte S { get; set; }

        public byte F { get; set; }

        public HsmsMessageType SType { get; set; }

        public byte PType { get; set; }

        public uint Context { get; set; }

        public bool ReplyExpected { get; set; }

        public HsmsHeader()
        {
        }

        public HsmsHeader(ByteBufferReader buffer)
        {
            Size = buffer.ReadU4();
            Device = buffer.ReadU2();
            var s = buffer.ReadByte();
            S = (byte)(s & 127);
            ReplyExpected = (s & 128) != 0;
            F = buffer.ReadByte();
            PType = buffer.ReadByte();
            SType = (HsmsMessageType)buffer.ReadByte();
            Context = buffer.ReadU4();
        }

        public void Write(ByteBufferWriter buffer)
        {
            buffer.Write(Size);
            buffer.Write(Device);
            byte s = (byte)(ReplyExpected ? S : S | 128);
            buffer.Write(s);
            buffer.Write(F);
            buffer.Write(PType);
            buffer.Write((byte)SType);
            buffer.Write(Context);
        }
    }
}