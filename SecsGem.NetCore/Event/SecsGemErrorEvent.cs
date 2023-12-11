namespace SecsGem.NetCore.Event
{
    public class SecsGemErrorEvent : SecsGemEvent
    {
        public string Message { get; set; }

        public SecsGemException Exception { get; set; }

        public override SecsGemEventType Event => SecsGemEventType.Error;
    }
}