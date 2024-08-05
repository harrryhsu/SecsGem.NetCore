using SecsGem.NetCore.Event.Common;

namespace SecsGem.NetCore.Event.Client
{
    public delegate Task SecsGemClientEventHandler(SecsGemClient sender, SecsGemEvent e);

    public interface ISecsGemClientEventHandler
    {
        /// <summary>
        /// Notification an alarm trigger, Triggered by S5F1
        /// </summary>
        Task Alarm(SecsGemAlarmEvent evt);

        /// <summary>
        /// Notification a collection event, Triggered by S6F11
        /// </summary>
        Task CollectionEvent(SecsGemCollectionEventEvent evt);

        /// <summary>
        /// Notification for error on the equipment
        /// </summary>
        Task NotifyException(SecsGemNotifyExceptionEvent evt);

        /// <summary>
        /// Notification for any unhandled message
        /// </summary>
        Task OrphanMessage(SecsGemClientOrphanMessageEvent evt);

        /// <summary>
        /// Request for displaying message on terminal
        /// Triggered by S10F1
        /// </summary>
        Task TerminalDisplay(SecsGemTerminalDisplayEvent evt);

        /// <summary>
        /// Triggered whenever there is a change in the kernel state,
        /// set evt.Accept to false will cancel the transition,
        /// if evt.Force is true, the evt.Accept has no effect, the message become notification only
        /// </summary>
        /// <param name="evt"></param>
        Task StateChange(SecsGemClientStateChangeEvent evt);

        /// <summary>
        /// Error event for any SecsGem or HSMS exception
        /// </summary>
        Task Error(SecsGemErrorEvent evt);

        /// <summary>
        /// Triggered when a hsms message is received
        /// </summary>
        /// <param name="evt"></param>
        Task Message(SecsGemMessageEvent evt);
    }

    public class SecsGemClientEventExecuter
    {
        private readonly ISecsGemClientEventHandler _handler;

        public SecsGemClientEventExecuter(ISecsGemClientEventHandler handler)
        {
            _handler = handler;
        }

        public async Task Execute(SecsGemEvent e)
        {
            switch (e.Event)
            {
                case SecsGemEventType.Alarm:
                    await _handler.Alarm(e as SecsGemAlarmEvent);
                    break;

                case SecsGemEventType.CollectionEvent:
                    await _handler.CollectionEvent(e as SecsGemCollectionEventEvent);
                    break;

                case SecsGemEventType.NotifyException:
                    await _handler.NotifyException(e as SecsGemNotifyExceptionEvent);
                    break;

                case SecsGemEventType.TerminalDisplay:
                    await _handler.TerminalDisplay(e as SecsGemTerminalDisplayEvent);
                    break;

                case SecsGemEventType.OrphanMessage:
                    await _handler.OrphanMessage(e as SecsGemClientOrphanMessageEvent);
                    break;

                case SecsGemEventType.StateChange:
                    await _handler.StateChange(e as SecsGemClientStateChangeEvent);
                    break;

                case SecsGemEventType.Error:
                    await _handler.Error(e as SecsGemErrorEvent);
                    break;

                case SecsGemEventType.Message:
                    await _handler.Message(e as SecsGemMessageEvent);
                    break;
            }
        }
    }
}