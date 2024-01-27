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
        public volatile bool IsSelected = false;

        public GemServerControlStateMachine ControlState { get; set; } = new();

        public volatile CommunicationStateModel CommunicationState = CommunicationStateModel.CommunicationDisconnected;

        public string Model { get; set; }

        public string Revision { get; set; }

        public bool IsCommunicationOnline => CommunicationState == CommunicationStateModel.CommunicationOnline;
    }
}