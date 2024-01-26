using SecsGem.NetCore.Event.Common;
using SecsGem.NetCore.Event.Server;
using SecsGem.NetCore.Feature.Server;
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
            if (ack == 0x0)
            {
                await _kernel.SetCommunicationState(CommunicationStateModel.CommunicationOnline, false);
                return false;
            }
            else
            {
                await _kernel.Emit(new SecsGemErrorEvent
                {
                    Message = "Remote Rejected Communication Establishment Request"
                });
                return true;
            }
        }

        public async Task<bool> TriggerAlarm(uint id, CancellationToken ct = default)
        {
            var alarm = _kernel.Feature.Alarms.FirstOrDefault(x => x.Id == id);
            if (alarm == null) throw new SecsGemException("Id not found");
            if (!alarm.Enabled || !_kernel.Device.ControlState.IsControlOnline) return false;

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

            return true;
        }

        public async Task<bool> SendHostDisplay(byte id, string text, CancellationToken ct = default)
        {
            if (!_kernel.Device.ControlState.IsControlOnline) return false;

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
            return code == 0x0;
        }

        public async Task<bool> SendCollectionEvent(uint id, CancellationToken ct = default)
        {
            if (!_kernel.Device.ControlState.IsControlOnline) return false;

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

            return true;
        }

        public async Task<bool> NotifyException(string id, Exception ex, string recoveryMessage, CancellationToken ct = default)
        {
            return await NotifyException(DateTime.Now, id, ex.GetType().Name, ex.Message, recoveryMessage, ct);
        }

        public async Task<bool> NotifyException(DateTime timestamp, string id, string type, string message, string recoveryMessage, CancellationToken ct = default)
        {
            if (!_kernel.Device.ControlState.IsControlOnline) return false;

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
                    .Build(),
                ct
            );

            return true;
        }

        public async Task Separate(CancellationToken ct = default)
        {
            try
            {
                if (_tcp.IsConnected)
                    await _tcp.SendAsync(
                        HsmsMessage.Builder
                            .Type(HsmsMessageType.SeparateReq)
                            .Build(),
                        ct
                    );
            }
            catch { }
        }

        public async Task Deselect(CancellationToken ct = default)
        {
            await _tcp.SendAsync(
                HsmsMessage.Builder
                    .Type(HsmsMessageType.DeselectReq)
                    .Build(),
                ct
            );
        }
    }
}