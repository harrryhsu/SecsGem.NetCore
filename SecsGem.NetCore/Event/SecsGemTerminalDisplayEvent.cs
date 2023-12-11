namespace SecsGem.NetCore.Event
{
    public class SecsGemTerminalDisplayEvent : SecsGemEvent
    {
        public override SecsGemEventType Event => SecsGemEventType.TerminalDisplay;

        public byte? Id { get; set; }

        public IEnumerable<string> Text { get; set; }

        public bool Return { get; set; }
    }
}