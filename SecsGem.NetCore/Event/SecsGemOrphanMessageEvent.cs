using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Event
{
    public class SecsGemOrphanMessageEvent : SecsGemEvent<HsmsMessage>
    {
        public override SecsGemEventType Event => SecsGemEventType.OrphanMessage;
    }
}