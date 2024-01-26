using SecsGem.NetCore.Event.Common;

namespace SecsGem.NetCore.Event.Server
{
    public class SecsGemInitEvent : SecsGemEvent
    {
        public override SecsGemEventType Event => SecsGemEventType.Init;
    }
}