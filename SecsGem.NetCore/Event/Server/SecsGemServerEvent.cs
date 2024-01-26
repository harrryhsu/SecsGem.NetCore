using SecsGem.NetCore.Event.Common;

namespace SecsGem.NetCore.Event.Server
{
    public delegate Task SecsGemEventHandler(SecsGemServer sender, SecsGemEvent e);

    public interface ISecsGemServerEventHandler
    {
        Task Init(SecsGemInitEvent evt);

        Task Stop(SecsGemStopEvent evt);

        Task GetStatusVariable(SecsGemGetStatusVariableEvent evt);

        Task GetDataVariable(SecsGemGetDataVariableEvent evt);

        Task GetEquipmentConstant(SecsGemGetEquipmentConstantEvent evt);

        Task SetEquipmentConstant(SecsGemSetEquipmentConstantEvent evt);

        Task CommandExecute(SecsGemCommandExecuteEvent evt);

        Task TerminalDisplay(SecsGemTerminalDisplayEvent evt);

        Task CommunicationStateChange(SecsGemCommunicationStateChangeEvent evt);

        Task OrphanMessage(SecsGemOrphanMessageEvent evt);

        Task Error(SecsGemErrorEvent evt);

        Task SetTime(SecsGemSetTimeEvent evt);
    }

    public class SecsGemServerEventHandlerExecuter
    {
        private readonly ISecsGemServerEventHandler _handler;

        public SecsGemServerEventHandlerExecuter(ISecsGemServerEventHandler handler)
        {
            _handler = handler;
        }

        public async Task Execute(SecsGemEvent e)
        {
            switch (e.Event)
            {
                case SecsGemEventType.Init:
                    await _handler.Init(e as SecsGemInitEvent);
                    break;

                case SecsGemEventType.SetTime:
                    await _handler.SetTime(e as SecsGemSetTimeEvent);
                    break;

                case SecsGemEventType.Stop:
                    await _handler.Stop(e as SecsGemStopEvent);
                    break;

                case SecsGemEventType.GetStatusVariable:
                    await _handler.GetStatusVariable(e as SecsGemGetStatusVariableEvent);
                    break;

                case SecsGemEventType.GetDataVariable:
                    await _handler.GetDataVariable(e as SecsGemGetDataVariableEvent);
                    break;

                case SecsGemEventType.GetEquipmentConstant:
                    await _handler.GetEquipmentConstant(e as SecsGemGetEquipmentConstantEvent);
                    break;

                case SecsGemEventType.SetEquipmentConstant:
                    await _handler.SetEquipmentConstant(e as SecsGemSetEquipmentConstantEvent);
                    break;

                case SecsGemEventType.CommandExecute:
                    await _handler.CommandExecute(e as SecsGemCommandExecuteEvent);
                    break;

                case SecsGemEventType.TerminalDisplay:
                    await _handler.TerminalDisplay(e as SecsGemTerminalDisplayEvent);
                    break;

                case SecsGemEventType.CommunicationStateChange:
                    await _handler.CommunicationStateChange(e as SecsGemCommunicationStateChangeEvent);
                    break;

                case SecsGemEventType.OrphanMessage:
                    await _handler.OrphanMessage(e as SecsGemOrphanMessageEvent);
                    break;

                case SecsGemEventType.Error:
                    await _handler.Error(e as SecsGemErrorEvent);
                    break;
            }
        }
    }
}