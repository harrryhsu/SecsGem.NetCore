using SecsGem.NetCore.Handler.Common;

namespace SecsGem.NetCore.Event.Common
{
    public class SecsGemServerOrphanMessageEvent : SecsGemEvent<SecsGemRequestContext<SecsGemServer>>
    {
        public override SecsGemEventType Event => SecsGemEventType.OrphanMessage;
    }
}