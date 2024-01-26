using SecsGem.NetCore.Event.Common;

namespace SecsGem.NetCore.Event.Server
{
    public class SecsGemStopEvent : SecsGemEvent
    {
        public override SecsGemEventType Event => SecsGemEventType.Stop;
    }
}