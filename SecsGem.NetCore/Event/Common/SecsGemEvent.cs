namespace SecsGem.NetCore.Event.Common
{
    public enum SecsGemEventType
    {
        Init,

        Stop,

        SetTime,

        GetStatusVariable,

        GetDataVariable,

        GetEquipmentConstant,

        SetEquipmentConstant,

        CommandExecute,

        TerminalDisplay,

        StateChange,

        OrphanMessage,

        Error,

        Alarm,

        CollectionEvent,

        NotifyException,
    }

    public abstract class SecsGemEvent
    {
        public abstract SecsGemEventType Event { get; }
    }

    public abstract class SecsGemEvent<TParam> : SecsGemEvent
    {
        public TParam Params { get; set; }
    }
}