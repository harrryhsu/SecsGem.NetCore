using SecsGem.NetCore.Feature;

namespace SecsGem.NetCore.Event
{
    public class SecsGemCommunicationStateChangeEvent : SecsGemEvent
    {
        public override SecsGemEventType Event => SecsGemEventType.CommunicationStateChange;

        public CommunicationStateModel OldState { get; set; }

        public CommunicationStateModel NewState { get; set; }

        public bool Return { get; set; }
    }
}