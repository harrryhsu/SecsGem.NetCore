using System.Net;

namespace SecsGem.NetCore
{
    public class SecsGemOption
    {
        public IPEndPoint Target { get; set; } = new(IPAddress.Any, 5000);

        public int TcpBufferSize { get; set; } = 4096;

        public bool ActiveConnect { get; set; } = false;

        public bool Debug { get; set; } = false;

        // T3 (Reply timeout)
        public int T3 { get; set; } = 3000;

        // T5 (Connection Separation Timeout)
        //public int T5 { get; set; } = 3000;

        // T6 (Control Transaction Timeout)
        //public int T6 { get; set; } = 3000;

        // T7 (Not Selected Timeout)
        public int T7 { get; set; } = 3000;

        // T8  (Byte Recv Timeout)
        public int T8 { get; set; } = 500;

        // Link Test Interval
        public int TLinkTest { get; set; } = 5000;
    }
}