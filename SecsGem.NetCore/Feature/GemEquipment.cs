namespace SecsGem.NetCore.Feature
{
    public enum ControlStateModel
    {
        ControlOffLine = 0,

        ControlOnlineLocal,

        ControlOnlineRemote,
    }

    public enum CommunicationStateModel
    {
        CommunicationDisconnected = 0,

        CommunicationOffline,

        CommunicationOnline,
    }

    public class GemDevice
    {
        public bool IsSelected { get; set; }

        public ControlStateModel ControlState { get; set; }

        public ControlStateModel InitControlState { get; set; } = ControlStateModel.ControlOnlineRemote;

        public CommunicationStateModel CommunicationState { get; set; }

        public string Model { get; set; }

        public string Revision { get; set; }

        public bool IsControlOnline => ControlState != ControlStateModel.ControlOffLine;

        public bool IsCommunicationOnline => CommunicationState == CommunicationStateModel.CommunicationOnline;
    }
}