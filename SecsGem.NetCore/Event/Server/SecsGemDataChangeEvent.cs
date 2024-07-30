using SecsGem.NetCore.Event.Common;

namespace SecsGem.NetCore.Event.Server
{
    public class SecsGemDataChangeEvent : SecsGemEvent
    {
        public override SecsGemEventType Event => SecsGemEventType.DataChange;
    }
}