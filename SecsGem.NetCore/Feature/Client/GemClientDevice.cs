using SecsGem.NetCore.Feature.Server;

namespace SecsGem.NetCore.Feature.Client
{
    public class GemClientDevice
    {
        public bool IsSelected { get; set; }

        public ControlStateModel ControlState { get; set; } = ControlStateModel.ControlHostOffLine;

        public CommunicationStateModel CommunicationState { get; set; }

        public string Model { get; set; }

        public string Revision { get; set; }

        public bool IsCommunicationOnline => CommunicationState == CommunicationStateModel.CommunicationOnline;
    }
}