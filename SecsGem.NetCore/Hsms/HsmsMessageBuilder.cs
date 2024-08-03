namespace SecsGem.NetCore.Hsms
{
    public class HsmsMessageBuilder
    {
        private static readonly Random _random = new();

        private HsmsMessageType _type = HsmsMessageType.DataMessage;

        private ushort _device;

        private uint _context = (uint)_random.NextInt64() % 10000;

        private byte _s;

        private byte _f;

        private bool _replyExpected;

        private int? _replyTimeout;

        private DataItem _item;

        public HsmsMessageBuilder Type(HsmsMessageType type)
        {
            _type = type;
            return this;
        }

        public HsmsMessageBuilder Device(ushort device)
        {
            _device = device;
            return this;
        }

        public HsmsMessageBuilder Context(uint context)
        {
            _context = context;
            return this;
        }

        public HsmsMessageBuilder Stream(byte s)
        {
            _s = s;
            return this;
        }

        public HsmsMessageBuilder Func(byte f)
        {
            _f = f;
            return this;
        }

        public HsmsMessageBuilder Reply(HsmsMessage msg)
        {
            _device = msg.Header.Device;
            _s = msg.Header.S;
            _f = (byte)(msg.Header.F + 1);
            _context = msg.Header.Context;
            return this;
        }

        public HsmsMessageBuilder ReplyExpected(bool reply = true)
        {
            _replyExpected = reply;
            return this;
        }

        public HsmsMessageBuilder Item(DataItem item)
        {
            _item = item;
            return this;
        }

        public HsmsMessageBuilder ReplyTimeout(int timeout)
        {
            _replyTimeout = timeout;
            return this;
        }

        public HsmsMessage Build()
        {
            return new HsmsMessage
            {
                Header = new HsmsHeader
                {
                    Context = _context,
                    Device = _device,
                    S = _s,
                    F = _f,
                    SType = _type,
                    ReplyExpected = _replyExpected,
                },
                Root = _item,
                ReplyTimeout = _replyTimeout
            };
        }
    }
}