using SecsGem.NetCore.Enum;
using SecsGem.NetCore.Error;
using SecsGem.NetCore.Event.Common;
using SecsGem.NetCore.Event.Server;
using SecsGem.NetCore.Hsms;
using SecsGem.NetCore.State.Server;
using TrafficCom.V3.Connection;

namespace SecsGem.NetCore.Function
{
    public class GemServerFunction
    {
        private readonly SecsGemServer _kernel;

        private readonly SecsGemTcpServer _tcp;

        public GemServerFunction(SecsGemServer kernel)
        {
            _kernel = kernel;
            _tcp = kernel._tcp;
        }

        /// <summary>
        /// S1F13 Establish communication to transition into control offline state
        /// </summary>
        /// <returns>If state transition succeeded</returns>
        /// <exception cref="SecsGemConnectionException"></exception>
        /// <exception cref="SecsGemTransactionException"></exception>
        public async Task<bool> CommunicationEstablish(CancellationToken ct = default)
        {
            var reply = await _tcp.SendAndWaitForReplyAsync(
                HsmsMessage.Builder
                    .Stream(1)
                    .Func(13)
                    .ReplyExpected()
                    .Build(),
                ct
            );

            var ack = reply.Root[0].GetBin();
            if (ack == 0)
            {
                await _kernel.State.TriggerAsync(GemServerStateTrigger.EstablishCommunication, true);
                return true;
            }
            else
            {
                await _kernel.Emit(new SecsGemErrorEvent
                {
                    Message = "Remote Rejected Communication Establishment Contextuest"
                });
                return false;
            }
        }

        /// <summary>
        /// S5F1 Trigger alarm, message is only sent if kernel state is readable and alarm is enabled
        /// </summary>
        /// <exception cref="SecsGemInvalidOperationException"></exception>
        /// <exception cref="SecsGemConnectionException"></exception>
        /// <exception cref="SecsGemTransactionException"></exception>
        public async Task TriggerAlarm(uint id, CancellationToken ct = default)
        {
            var alarm = _kernel.Feature.Alarms.FirstOrDefault(x => x.Id == id);
            if (alarm == null) throw new SecsGemException("Id not found");
            if (!_kernel.State.IsReadable || !alarm.Enabled) throw new SecsGemInvalidOperationException();

            await _tcp.SendAndWaitForReplyAsync(
                HsmsMessage.Builder
                    .Stream(5)
                    .Func(1)
                    .ReplyExpected(true)
                    .Item(new ListDataItem(
                        new BinDataItem(128),
                        new U4DataItem(alarm.Id),
                        new ADataItem(alarm.Description)
                    ))
                    .Build(),
                ct
            );
        }

        /// <summary>
        /// S10F1 Send single line terminal display
        /// </summary>
        /// <returns>Terminal display result</returns>
        /// <exception cref="SecsGemInvalidOperationException"></exception>
        /// <exception cref="SecsGemConnectionException"></exception>
        /// <exception cref="SecsGemTransactionException"></exception>
        public async Task<SECS_RESPONSE.ACKC10> SendTerminal(byte id, string text, CancellationToken ct = default)
        {
            if (!_kernel.State.IsReadable) throw new SecsGemInvalidOperationException();

            var reply = await _tcp.SendAndWaitForReplyAsync(
                  HsmsMessage.Builder
                      .Stream(10)
                      .Func(1)
                      .ReplyExpected()
                      .Item(new ListDataItem(
                          new BinDataItem(id),
                          new ADataItem(text)
                      ))
                      .Build(),
                  ct
               );

            var code = reply.Root.GetBin();
            return (SECS_RESPONSE.ACKC10)code;
        }

        /// <summary>
        /// S6F11 Send collection event, SecsGemGetDataVariableEvent is triggered to populate the collection event data variables
        /// </summary>
        /// <exception cref="SecsGemInvalidOperationException"></exception>
        /// <exception cref="SecsGemConnectionException"></exception>
        /// <exception cref="SecsGemTransactionException"></exception>
        public async Task SendCollectionEvent(uint id, CancellationToken ct = default)
        {
            if (!_kernel.State.IsReadable) throw new SecsGemInvalidOperationException();

            var ce = _kernel.Feature.CollectionEvents.FirstOrDefault(x => x.Id == id);
            if (ce == null) throw new SecsGemException("Id not found");

            await _kernel.Emit(new SecsGemGetDataVariableEvent
            {
                Params = ce.CollectionReports.SelectMany(x => x.DataVariables)
            });

            await _tcp.SendAndWaitForReplyAsync(
                HsmsMessage.Builder
                    .Stream(6)
                    .Func(11)
                    .ReplyExpected()
                    .Item(new ListDataItem(
                        new U4DataItem(0),
                        new U4DataItem(ce.Id),
                        new ListDataItem(
                            ce.CollectionReports
                                .Select(x => new ListDataItem(
                                    new U4DataItem(x.Id),
                                    new ListDataItem(
                                        x.DataVariables
                                            .Select(x => new ADataItem(x.Value))
                                            .ToArray()
                                    )
                                )).ToArray()
                        )
                    ))
                    .Build(),
                ct
            );
        }

        /// <summary>
        /// S5F9 Notify host of an equipment exception
        /// </summary>
        /// <exception cref="SecsGemInvalidOperationException"></exception>
        /// <exception cref="SecsGemConnectionException"></exception>
        /// <exception cref="SecsGemTransactionException"></exception>
        public async Task NotifyException(string id, Exception ex, string recoveryMessage, DateTime timestamp = default, CancellationToken ct = default)
        {
            await NotifyException(id, ex.GetType().Name, ex.Message, recoveryMessage, timestamp, ct);
        }

        /// <summary>
        /// S5F9 Notify host of an equipment exception
        /// </summary>
        /// <exception cref="SecsGemInvalidOperationException"></exception>
        /// <exception cref="SecsGemConnectionException"></exception>
        /// <exception cref="SecsGemTransactionException"></exception>
        public async Task NotifyException(string id, string type, string message, string recoveryMessage, DateTime timestamp = default, CancellationToken ct = default)
        {
            if (!_kernel.State.IsReadable) throw new SecsGemInvalidOperationException();
            if (timestamp == default) timestamp = DateTime.Now;

            await _tcp.SendAndWaitForReplyAsync(
                HsmsMessage.Builder
                    .Stream(5)
                    .Func(9)
                    .ReplyExpected(true)
                    .Item(new ListDataItem(
                        new ADataItem(timestamp.ToString("O")),
                        new ADataItem(id),
                        new ADataItem(type),
                        new ADataItem(message),
                        new ListDataItem(
                            recoveryMessage
                                .Chunk(40)
                                .Select(x => new ADataItem(new string(x)))
                                .ToArray()
                        )
                    ))
                    .Build(),
                ct
            );
        }

        /// <summary>
        /// Disconnect immediately
        /// </summary>
        /// <exception cref="SecsGemInvalidOperationException"></exception>
        /// <exception cref="SecsGemConnectionException"></exception>
        /// <exception cref="SecsGemTransactionException"></exception>
        public async Task Separate(CancellationToken ct = default)
        {
            if (_kernel.State.IsExact(GemServerStateModel.Disconnected))
                throw new SecsGemInvalidOperationException();

            await _tcp.SendAsync(
                HsmsMessage.Builder
                    .Type(HsmsMessageType.SeparateReq)
                    .Build(),
                ct
            );
        }

        /// <summary>
        /// Transition the local state machine to online remote
        /// </summary>
        /// <returns>If state transition succeeded</returns>
        public async Task<bool> GoOnlineRemote()
        {
            return await _kernel.State.TriggerAsync(GemServerStateTrigger.GoOnlineRemote, false);
        }

        /// <summary>
        /// Transition the local state machine to online local
        /// </summary>
        /// <returns>If state transition succeeded</returns>
        public async Task<bool> GoOnlineLocal()
        {
            return await _kernel.State.TriggerAsync(GemServerStateTrigger.GoOnlineLocal, false);
        }

        /// <summary>
        /// Send HSMS message to host
        /// </summary>
        /// <returns>HSMS message response</returns>
        /// <exception cref="SecsGemConnectionException"></exception>
        /// <exception cref="SecsGemTransactionException"></exception>
        public async Task<HsmsMessage> Send(HsmsMessage message, CancellationToken ct = default)
        {
            return await _tcp.SendAndWaitForReplyAsync(message, ct);
        }
    }
}