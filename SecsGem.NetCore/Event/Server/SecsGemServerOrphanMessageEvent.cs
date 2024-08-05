using SecsGem.NetCore.Handler.Common;

namespace SecsGem.NetCore.Event.Common
{
    public class SecsGemServerOrphanMessageEvent : SecsGemEvent
    {
        public override SecsGemEventType Event => SecsGemEventType.OrphanMessage;

        public SecsGemRequestContext<SecsGemServer> Context { get; set; }
    }
}