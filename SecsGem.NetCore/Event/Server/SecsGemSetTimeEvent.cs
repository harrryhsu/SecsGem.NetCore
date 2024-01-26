using SecsGem.NetCore.Event.Common;

namespace SecsGem.NetCore.Event.Server
{
    public class SecsGemSetTimeEvent : SecsGemEvent
    {
        public override SecsGemEventType Event => SecsGemEventType.SetTime;

        public string Time { get; set; }

        public bool Success { get; set; } = false;
    }
}