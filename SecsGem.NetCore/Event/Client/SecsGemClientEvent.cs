using SecsGem.NetCore.Event.Common;

namespace SecsGem.NetCore.Event.Client
{
    public delegate Task SecsGemClientEventHandler(SecsGemClient sender, SecsGemEvent e);

    public interface ISecsGemClientEventHandler
    {
        Task Alarm(SecsGemAlarmEvent evt);

        Task CollectionEvent(SecsGemCollectionEventEvent evt);

        Task NotifyException(SecsGemNotifyExceptionEvent evt);

        Task OrphanMessage(SecsGemClientOrphanMessageEvent evt);

        Task TerminalDisplay(SecsGemTerminalDisplayEvent evt);

        Task StateChange(SecsGemClientStateChangeEvent evt);

        Task Error(SecsGemErrorEvent evt);
    }

    public class SecsGemClientEventHandlerExecuter
    {
        private readonly ISecsGemClientEventHandler _handler;

        public SecsGemClientEventHandlerExecuter(ISecsGemClientEventHandler handler)
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
            }
        }
    }
}