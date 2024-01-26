using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Event.Common
{
    public class SecsGemOrphanMessageEvent : SecsGemEvent<HsmsMessage>
    {
        public override SecsGemEventType Event => SecsGemEventType.OrphanMessage;
    }
}