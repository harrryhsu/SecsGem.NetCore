using SecsGem.NetCore.Handler.Common;

namespace SecsGem.NetCore.Event.Common
{
    public class SecsGemClientOrphanMessageEvent : SecsGemEvent
    {
        public override SecsGemEventType Event => SecsGemEventType.OrphanMessage;

        public SecsGemRequestContext<SecsGemClient> Context { get; set; }
    }
}