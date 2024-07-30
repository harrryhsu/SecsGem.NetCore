using SecsGem.NetCore.Hsms;
using System.Diagnostics;

namespace SecsGem.NetCore.Handler.Common
{
    public abstract class SecsGemHandlerCache
    {
        public Type HandlerType { get; set; }

        public abstract bool IsMatch(HsmsMessage message, bool IsRequest);
    }

    [DebuggerDisplay("{FunctionType} S{Stream}F{Function}")]
    public class SecsGemStreamHandlerCache : SecsGemHandlerCache
    {
        public SecsGemFunctionType FunctionType { get; set; }

        public int Stream { get; set; }

        public int Function { get; set; }

        public override bool IsMatch(HsmsMessage message, bool IsRequest)
        {
            return !IsRequest &&
                Stream == message.Header.S &&
                Function == message.Header.F;
        }
    }

    [DebuggerDisplay("{MessageType}")]
    public class SecsGemRequestHandlerCache : SecsGemHandlerCache
    {
        public HsmsMessageType MessageType { get; set; }

        public override bool IsMatch(HsmsMessage message, bool IsRequest)
        {
            return IsRequest && MessageType == message.Header.SType;
        }
    }
}