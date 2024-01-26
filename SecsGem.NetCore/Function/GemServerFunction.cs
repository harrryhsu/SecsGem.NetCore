using SecsGem.NetCore.Event.Common;
using SecsGem.NetCore.Event.Server;
using SecsGem.NetCore.Hsms;
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

        public async Task<bool> TriggerAlarm(uint id)
        {
            var alarm = _kernel.Feature.Alarms.FirstOrDefault(x => x.Id == id);
            if (alarm == null) throw new SecsGemException("Id not found");
            if (!alarm.Enabled || !_kernel.Device.ControlState.IsControlOnline) return false;

            try
            {
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
                        .Build()
                );

                return true;
            }
            catch (SecsGemTransactionException ex)
            {
                await _kernel.Emit(new SecsGemErrorEvent
                {
                    Message = "TriggerAlarm Error",
                    Exception = ex,
                });
            }
            catch (SecsGemConnectionException)
            {
            }

            return false;
        }

        public async Task<bool> SendHostDisplay(byte id, string text)
        {
            if (!_kernel.Device.ControlState.IsControlOnline) return false;

            try
            {
                var reply = await _tcp.SendAndWaitForReplyAsync(
                   HsmsMessage.Builder
                       .Stream(10)
                       .Func(1)
                       .ReplyExpected()
                       .Item(new ListDataItem(
                           new BinDataItem(id),
                           new ADataItem(text)
                       ))
                       .Build()
                );

                var code = reply.Root.GetBin();
                return code == 0x0;
            }
            catch (SecsGemTransactionException ex)
            {
                await _kernel.Emit(new SecsGemErrorEvent
                {
                    Message = "SendHostDisplay Error",
                    Exception = ex,
                });
            }
            catch (SecsGemConnectionException)
            {
            }

            return false;
        }

        public async Task<bool> SendCollectionEvent(uint id)
        {
            if (!_kernel.Device.ControlState.IsControlOnline) return false;

            var ce = _kernel.Feature.CollectionEvents.FirstOrDefault(x => x.Id == id);
            if (ce == null) throw new SecsGemException("Id not found");

            await _kernel.Emit(new SecsGemGetDataVariableEvent
            {
                Params = ce.CollectionReports.SelectMany(x => x.DataVariables)
            });

            try
            {
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
                        .Build()
                );

                return true;
            }
            catch (SecsGemTransactionException ex)
            {
                await _kernel.Emit(new SecsGemErrorEvent
                {
                    Message = "SendCollectionEvent Error",
                    Exception = ex,
                });
            }
            catch (SecsGemConnectionException)
            {
            }

            return false;
        }

        public async Task<bool> NotifyException(string id, Exception ex, string recoveryMessage = "")
        {
            return await NotifyException(DateTime.Now, id, ex.GetType().Name, ex.Message, recoveryMessage);
        }

        public async Task<bool> NotifyException(DateTime timestamp, string id, string type, string message, string recoveryMessage = "")
        {
            if (!_kernel.Device.ControlState.IsControlOnline) return false;

            try
            {
                await _tcp.SendAndWaitForReplyAsync(
                    HsmsMessage.Builder
                        .Stream(5)
                        .Func(9)
                        .ReplyExpected(true)
                        .Item(new ListDataItem(
                            new ADataItem(timestamp.ToString("YYYY-MM-ddTHH:mm:ss")),
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
                        .Build()
                );

                return true;
            }
            catch (SecsGemTransactionException ex)
            {
                await _kernel.Emit(new SecsGemErrorEvent
                {
                    Message = "NotifyException Error",
                    Exception = ex,
                });
            }
            catch (SecsGemConnectionException)
            {
            }

            return false;
        }

        public async Task Teardown()
        {
            if (_tcp.IsConnected)
                await _tcp.SendAsync(
                    HsmsMessage.Builder
                        .Type(HsmsMessageType.SeparateReq)
                        .Build()
                );
        }
    }
}