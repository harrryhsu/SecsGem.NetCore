namespace SecsGem.NetCore.State.Server
{
    public enum GemServerStateModel
    {
        Disconnected = 0,

        Connected,

        Selected,

        ControlOffLine,

        ControlOnlineLocal,

        ControlOnlineRemote,
    }
}