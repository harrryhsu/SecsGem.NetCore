using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler.Common
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SecsGemMessageAttribute : Attribute
    {
        public HsmsMessageType Type { get; set; }

        public SecsGemMessageAttribute(HsmsMessageType type)
        {
            Type = type;
        }
    }
}