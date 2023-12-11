using SecsGem.NetCore.Feature;

namespace SecsGem.NetCore.Event
{
    public class SecsGemControlStateChangeEvent : SecsGemEvent
    {
        public override SecsGemEventType Event => SecsGemEventType.ControlStateChange;

        public ControlStateModel OldState { get; set; }

        public ControlStateModel NewState { get; set; }

        public bool Return { get; set; }
    }
}