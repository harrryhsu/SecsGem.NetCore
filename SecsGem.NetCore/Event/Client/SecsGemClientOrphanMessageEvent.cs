using SecsGem.NetCore.Handler.Common;

namespace SecsGem.NetCore.Event.Common
{
    public class SecsGemClientOrphanMessageEvent : SecsGemEvent<SecsGemRequestContext<SecsGemClient>>
    {
        public override SecsGemEventType Event => SecsGemEventType.OrphanMessage;
    }
}