using SecsGem.NetCore.Event.Common;
using SecsGem.NetCore.Feature.Server;

namespace SecsGem.NetCore.Event.Server
{
    public class SecsGemServerStateChangeEvent : SecsGemEvent
    {
        public override SecsGemEventType Event => SecsGemEventType.StateChange;

        public GemServerStateModel OldState { get; set; }

        public GemServerStateModel NewState { get; set; }

        public bool Accept { get; set; } = true;
    }
}