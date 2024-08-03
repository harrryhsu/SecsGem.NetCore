namespace SecsGem.NetCore.State.Server
{
    public enum GemServerStateTrigger
    {
        Connect,

        Disconnect,

        Select,

        Deselect,

        EstablishCommunication,

        GoOnline,

        GoOffline,

        GoOnlineLocal,

        GoOnlineRemote,
    }
}