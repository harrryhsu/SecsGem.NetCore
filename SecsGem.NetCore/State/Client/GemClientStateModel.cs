namespace SecsGem.NetCore.State.Client
{
    public enum GemClientStateModel
    {
        Disconnected = 0,

        Connected,

        Selected,

        ControlOffLine,

        ControlOnline,
    }
}