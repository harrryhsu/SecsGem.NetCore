using SecsGem.NetCore.Enum;

namespace SecsGem.NetCore.Event.Common
{
    public class SecsGemTerminalDisplayEvent : SecsGemEvent
    {
        public override SecsGemEventType Event => SecsGemEventType.TerminalDisplay;

        public byte Id { get; set; }

        public bool IsBroadcast { get; set; }

        public IEnumerable<string> Texts { get; set; }

        public SECS_RESPONSE.ACKC10 Return { get; set; }
    }
}