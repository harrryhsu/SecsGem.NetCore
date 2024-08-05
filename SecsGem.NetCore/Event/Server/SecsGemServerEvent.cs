using SecsGem.NetCore.Event.Common;

namespace SecsGem.NetCore.Event.Server
{
    public delegate Task SecsGemEventHandler(SecsGemServer sender, SecsGemEvent e);

    public interface ISecsGemServerEventHandler
    {
        /// <summary>
        /// On SecsGem server start for initializing features and equipment data
        /// </summary>
        /// <param name="evt"></param>
        Task Init(SecsGemInitEvent evt);

        /// <summary>
        /// On SecsGem server stop
        /// </summary>
        /// <param name="evt"></param>
        Task Stop(SecsGemStopEvent evt);

        /// <summary>
        /// Request to populate Status Variables in evt.Params
        /// Triggered by S1F3
        /// </summary>
        /// <param name="evt"></param>
        Task GetStatusVariable(SecsGemGetStatusVariableEvent evt);

        /// <summary>
        /// Request to populate Data Variables in evt.Params
        /// Triggered by S6F15 or equipment initiated a SendCollectionEvent
        /// </summary>
        /// <param name="evt"></param>
        Task GetDataVariable(SecsGemGetDataVariableEvent evt);

        /// <summary>
        /// Request to populate Equipment Constants in evt.Params, Triggered by S2F13
        /// </summary>
        /// <param name="evt"></param>
        Task GetEquipmentConstant(SecsGemGetEquipmentConstantEvent evt);

        Task SetEquipmentConstant(SecsGemSetEquipmentConstantEvent evt);

        /// <summary>
        /// Command execute, Triggered by S2F41
        /// </summary>
        /// <param name="evt"></param>
        Task CommandExecute(SecsGemCommandExecuteEvent evt);

        /// <summary>
        /// Request for displaying message on terminal
        /// Triggered by S10F3/S10F5/S10F9
        /// </summary>
        /// <param name="evt"></param>
        Task TerminalDisplay(SecsGemTerminalDisplayEvent evt);

        /// <summary>
        /// Triggered whenever there is a change in the kernel state,
        /// set evt.Accept to false will cancel the transition,
        /// if evt.Force is true, the evt.Accept has no effect, the message become notification only
        /// </summary>
        /// <param name="evt"></param>
        Task StateChange(SecsGemServerStateChangeEvent evt);

        /// <summary>
        /// Notification for any unhandled message
        /// </summary>
        /// <param name="evt"></param>
        Task OrphanMessage(SecsGemServerOrphanMessageEvent evt);

        /// <summary>
        /// Error event for any SecsGem or HSMS exception
        /// </summary>
        /// <param name="evt"></param>
        Task Error(SecsGemErrorEvent evt);

        /// <summary>
        /// Set time request, Triggered by S2F31
        /// </summary>
        /// <param name="evt"></param>
        Task SetTime(SecsGemSetTimeEvent evt);

        /// <summary>
        /// Triggered whenever there is a data change,
        /// this event is used to notify for saving the data
        /// </summary>
        /// <param name="evt"></param>
        Task DataChange(SecsGemDataChangeEvent evt);

        /// <summary>
        /// Triggered when a hsms message is received
        /// </summary>
        /// <param name="evt"></param>
        Task Message(SecsGemMessageEvent evt);
    }

    public class SecsGemServerEventExecuter
    {
        private readonly ISecsGemServerEventHandler _handler;

        public SecsGemServerEventExecuter(ISecsGemServerEventHandler handler)
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

                case SecsGemEventType.StateChange:
                    await _handler.StateChange(e as SecsGemServerStateChangeEvent);
                    break;

                case SecsGemEventType.OrphanMessage:
                    await _handler.OrphanMessage(e as SecsGemServerOrphanMessageEvent);
                    break;

                case SecsGemEventType.Error:
                    await _handler.Error(e as SecsGemErrorEvent);
                    break;

                case SecsGemEventType.DataChange:
                    await _handler.DataChange(e as SecsGemDataChangeEvent);
                    break;

                case SecsGemEventType.Message:
                    await _handler.Message(e as SecsGemMessageEvent);
                    break;
            }
        }
    }
}