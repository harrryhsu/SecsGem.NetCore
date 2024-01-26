using SecsGem.NetCore.Event.Common;
using SecsGem.NetCore.Feature.Server;

namespace SecsGem.NetCore.Event.Server
{
    public class SecsGemCommunicationStateChangeEvent : SecsGemEvent
    {
        public override SecsGemEventType Event => SecsGemEventType.CommunicationStateChange;

        public CommunicationStateModel OldState { get; set; }

        public CommunicationStateModel NewState { get; set; }

        public bool Accept { get; set; } = true;
    }
}