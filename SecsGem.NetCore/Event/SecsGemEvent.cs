namespace SecsGem.NetCore.Event
{
    public delegate Task SecsGemEventHandler(SecsGemKernel sender, SecsGemEvent e);

    public enum SecsGemEventType
    {
        Init,

        Stop,

        GetStatusVariable,

        GetDataVariable,

        GetEquipmentConstant,

        SetEquipmentConstant,

        CommandExecute,

        TerminalDisplay,

        ControlStateChange,

        CommunicationStateChange,

        OrphanMessage,

        Error,
    }

    public abstract class SecsGemEvent
    {
        public abstract SecsGemEventType Event { get; }
    }

    public abstract class SecsGemEvent<TParam> : SecsGemEvent
    {
        public TParam Params { get; set; }
    }

    public interface ISecsGemEventHandler
    {
        Task Init(SecsGemInitEvent evt);

        Task Stop(SecsGemStopEvent evt);

        Task GetStatusVariable(SecsGemGetStatusVariableEvent evt);

        Task GetDataVariable(SecsGemGetDataVariableEvent evt);

        Task GetEquipmentConstant(SecsGemGetEquipmentConstantEvent evt);

        Task SetEquipmentConstant(SecsGemSetEquipmentConstantEvent evt);

        Task CommandExecute(SecsGemCommandExecuteEvent evt);

        Task TerminalDisplay(SecsGemTerminalDisplayEvent evt);

        Task ControlStateChange(SecsGemControlStateChangeEvent evt);

        Task CommunicationStateChange(SecsGemCommunicationStateChangeEvent evt);

        Task OrphanMessage(SecsGemOrphanMessageEvent evt);

        Task Error(SecsGemErrorEvent evt);
    }
}