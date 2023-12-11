namespace SecsGem.NetCore.Event
{
    public class SecsGemStopEvent : SecsGemEvent
    {
        public override SecsGemEventType Event => SecsGemEventType.Stop;
    }
}