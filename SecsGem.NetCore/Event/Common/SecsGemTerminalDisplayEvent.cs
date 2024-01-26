namespace SecsGem.NetCore.Event.Common
{
    public enum SecsGemTerminalDisplayResult
    {
        Accept = 0,

        Denied,

        NotAvailable
    }

    public class SecsGemTerminalDisplayEvent : SecsGemEvent
    {
        public override SecsGemEventType Event => SecsGemEventType.TerminalDisplay;

        public byte? Id { get; set; }

        public IEnumerable<string> Text { get; set; }

        public SecsGemTerminalDisplayResult Return { get; set; }
    }
}