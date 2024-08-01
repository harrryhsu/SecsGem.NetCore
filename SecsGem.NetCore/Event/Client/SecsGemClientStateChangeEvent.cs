using SecsGem.NetCore.Event.Common;
using SecsGem.NetCore.Feature.Client;

namespace SecsGem.NetCore.Event.Client
{
    public class SecsGemClientStateChangeEvent : SecsGemEvent
    {
        public override SecsGemEventType Event => SecsGemEventType.StateChange;

        public GemClientStateModel OldState { get; set; }

        public GemClientStateTrigger Trigger { get; set; }

        public GemClientStateModel NewState { get; set; }

        public bool Force { get; set; }

        public bool Accept { get; set; } = true;
    }
}