using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Event.Common
{
    public class SecsGemMessageEvent : SecsGemEvent
    {
        public override SecsGemEventType Event => SecsGemEventType.Message;

        public HsmsMessage Message { get; set; }
    }
}