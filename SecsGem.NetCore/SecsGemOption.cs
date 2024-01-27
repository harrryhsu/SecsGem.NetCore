using System.Net;

namespace SecsGem.NetCore
{
    public class SecsGemOption
    {
        /// <summary>
        /// SecsGem target network interface
        /// </summary>
        public IPEndPoint Target { get; set; } = new(IPAddress.Any, 5000);

        /// <summary>
        /// Tcp receive buffer size, send buffer is dynamic
        /// </summary>
        public int TcpBufferSize { get; set; } = 4096;

        /// <summary>
        /// If client should initiate S1F13,
        /// Can also be initialted by calling SecsGemClient.Function.CommunicationEstablish
        /// </summary>
        public bool ActiveConnect { get; set; } = false;

        /// <summary>
        /// Enable debug log through Logger
        /// </summary>
        public bool Debug { get; set; } = false;

        /// <summary>
        /// Debug logger
        /// </summary>
        public Action<string> Logger { get; set; } = Console.WriteLine;

        /// <summary>
        /// Message reply timeout
        /// </summary>
        public int T3 { get; set; } = 1000;

        /// <summary>
        /// Not selected timeout, only has effect for SecsGemServer
        /// </summary>
        public int T7 { get; set; } = 1000;

        /// <summary>
        /// Byte Recv Timeout
        /// </summary>
        public int T8 { get; set; } = 500;

        /// <summary>
        /// Link Test Interval
        /// </summary>
        public int TLinkTest { get; set; } = 5000;

        internal void DebugLog(string message)
        {
            if (Debug) Logger(message);
        }
    }
}