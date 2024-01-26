namespace SecsGem.NetCore.Feature.Server
{
    public enum CommunicationStateModel
    {
        CommunicationDisconnected = 0,

        CommunicationOffline,

        CommunicationOnline,
    }

    public class GemServerDevice
    {
        public bool IsSelected { get; set; }

        public GemServerControlStateMachine ControlState { get; set; } = new();

        public CommunicationStateModel CommunicationState { get; set; }

        public string Model { get; set; }

        public string Revision { get; set; }

        public bool IsCommunicationOnline => CommunicationState == CommunicationStateModel.CommunicationOnline;
    }
}