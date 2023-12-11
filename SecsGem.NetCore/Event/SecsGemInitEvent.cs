namespace SecsGem.NetCore.Event
{
    public class SecsGemInitEvent : SecsGemEvent
    {
        public override SecsGemEventType Event => SecsGemEventType.Init;
    }
}