using SecsGem.NetCore.Event.Common;
using SecsGem.NetCore.Feature.Server;

namespace SecsGem.NetCore.Event.Server
{
    public class SecsGemControlStateChangeEvent : SecsGemEvent
    {
        public override SecsGemEventType Event => SecsGemEventType.ControlStateChange;

        public ControlStateModel OldState { get; set; }

        public ControlStateModel NewState { get; set; }

        public bool Return { get; set; }
    }
}