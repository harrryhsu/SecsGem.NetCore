using SecsGem.NetCore.Hsms;
using System.Diagnostics;

namespace SecsGem.NetCore.Handler.Common
{
    public abstract class SecsGemHandlerCache
    {
        public Type HandlerType { get; set; }

        public virtual bool IsMatch(HsmsMessageType type)
        {
            return false;
        }

        public virtual bool IsMatch(byte s, byte f)
        {
            return false;
        }
    }

    [DebuggerDisplay("{FunctionType} S{Stream}F{Function}")]
    public class SecsGemStreamHandlerCache : SecsGemHandlerCache
    {
        public SecsGemFunctionType FunctionType { get; set; }

        public int Stream { get; set; }

        public int Function { get; set; }

        public override bool IsMatch(byte s, byte f)
        {
            return Stream == s && Function == f;
        }
    }

    [DebuggerDisplay("{MessageType}")]
    public class SecsGemRequestHandlerCache : SecsGemHandlerCache
    {
        public HsmsMessageType MessageType { get; set; }

        public override bool IsMatch(HsmsMessageType type)
        {
            return MessageType == type;
        }
    }
}