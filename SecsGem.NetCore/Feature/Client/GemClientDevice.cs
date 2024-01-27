using SecsGem.NetCore.Feature.Server;

namespace SecsGem.NetCore.Feature.Client
{
    public class GemClientDevice
    {
        public volatile bool IsSelected;

        public volatile ControlStateModel ControlState = ControlStateModel.ControlHostOffLine;

        public volatile CommunicationStateModel CommunicationState = CommunicationStateModel.CommunicationDisconnected;

        public string Model { get; set; }

        public string Revision { get; set; }

        public bool IsCommunicationOnline => CommunicationState == CommunicationStateModel.CommunicationOnline;
    }
}