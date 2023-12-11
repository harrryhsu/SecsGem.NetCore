using SecsGem.NetCore.Connection;
using SecsGem.NetCore.Event;
using SecsGem.NetCore.Feature;
using SecsGem.NetCore.Handler;
using SecsGem.NetCore.Helper;
using SecsGem.NetCore.Hsms;
using SecsGem.NetCore.Mutex;
using TrafficCom.V3.Connection;

namespace SecsGem.NetCore
{
    public class SecsGemKernel : IHostedService
    {
        public SecsGemFeature Feature { get; protected set; } = new();

        public GemDevice Device { get; protected set; } = new();

        public event SecsGemEventHandler OnEvent;

        private readonly CancellationTokenSource _cts = new();

        private readonly SecsGemOption _option;

        private readonly SecsGemTcpServer _tcp;

        private readonly SecsGemRequestHandler _requestHandler = new();

        private readonly AsyncExecutionLock _lock = new();

        private TcpConnection _host;

        public SecsGemKernel(SecsGemOption option)
        {
            if (option.Target == null) throw new ArgumentException("Target");

            _option = option;
            _tcp = new(_option);
            _tcp.OnMessageReceived += OnTcpMessageReceived;
            _tcp.OnConnection += OnTcpConnection;
            _tcp.OnError += OnTcpError;
        }

        private async Task OnTcpMessageReceived(SecsGemTcpClient sender, TcpConnection con, HsmsMessage message)
        {
            await _lock.ExecuteAsync(async () =>
            {
                try
                {
                    await _requestHandler.Handle(sender, con, this, message);
                }
                catch (Exception ex)
                {
                    await Emit(new SecsGemErrorEvent
                    {
                        Message = "Error during request handling",
                        Exception = new SecsGemException("Error during request handling", ex),
                    });
                }
            });
        }

        private async Task OnTcpConnection(SecsGemTcpClient sender, TcpConnection con)
        {
            await _lock.ExecuteAsync(async () =>
            {
                if (_host != null)
                {
                    con.Close();
                }
                else
                {
                    _host = con;
                    _ = Task.Run(() => SecsGemWorker(con));
                }
            });
        }

        private async Task OnTcpError(SecsGemTcpClient sender, SecsGemException ex)
        {
            await Emit(new SecsGemErrorEvent
            {
                Message = "Tcp Error",
                Exception = ex,
            });
        }

        internal async Task<TEvent> Emit<TEvent>(TEvent e) where TEvent : SecsGemEvent
        {
            try
            {
                await OnEvent?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                if (e.Event == SecsGemEventType.Error)
                {
                    throw;
                }
                else
                {
                    await Emit(new SecsGemErrorEvent
                    {
                        Message = "Event Handler Error",
                        Exception = new SecsGemException("Event Handler Error", ex),
                    });
                }
            }

            return e;
        }

        internal async Task Disconnect()
        {
            await SetCommunicationState(CommunicationStateModel.CommunicationDisconnected);
            await SetControlState(ControlStateModel.ControlOffLine);
            Device.IsSelected = false;
            _host.Close();
            _host = null;
        }

        private async Task SecsGemWorker(TcpConnection con)
        {
            try
            {
                try
                {
                    if (_option.T7 < 100) _option.T7 = 100;
                    await TaskHelper.WaitFor(() => Device.CommunicationState != CommunicationStateModel.CommunicationDisconnected, 10, _option.T7 / 10, _cts.Token);
                    Device.IsSelected = true;
                }
                catch (TimeoutException)
                {
                    await Emit(new SecsGemErrorEvent
                    {
                        Exception = new SecsGemConnectionException("T7 Timeout") { Code = "timer_timeout" },
                    });
                    await Disconnect();
                    return;
                }

                while (!_cts.IsCancellationRequested && con.TcpClient.Connected)
                {
                    if (Device.CommunicationState == CommunicationStateModel.CommunicationOffline)
                    {
                        if (_option.ActiveConnect)
                        {
                            var reply = await _tcp.SendAndWaitForReplyAsync(
                                con,
                                HsmsMessage.Builder
                                    .Stream(1)
                                    .Func(13)
                                    .Build()
                            );

                            var ack = reply.Root[0].GetBin();
                            if (ack == 0x0)
                            {
                                await SetCommunicationState(CommunicationStateModel.CommunicationOnline);
                                await SetControlState(Device.InitControlState);
                            }
                            else
                            {
                                await Emit(new SecsGemErrorEvent
                                {
                                    Exception = new SecsGemConnectionException("Remote Rejected Communication Establish Request")
                                    {
                                        Code = "remote_reject_online"
                                    },
                                });
                                await Disconnect();
                                return;
                            }
                        }
                        else
                        {
                            await Task.Delay(1000);
                        }
                    }
                    else
                    {
                        var reply = await _tcp.SendAndWaitForReplyAsync(
                            con,
                            HsmsMessage.Builder
                                .Type(HsmsMessageType.LinkTestReq)
                                .Build()
                        );

                        await Task.Delay(_option.TLinkTest);
                    }
                }
            }
            catch (SecsGemConnectionException ex)
            {
                if (ex.Code != "not_connected")
                {
                    await Emit(new SecsGemErrorEvent
                    {
                        Exception = ex,
                    });
                }

                await Disconnect();
            }
            catch (Exception ex)
            {
                await Emit(new SecsGemErrorEvent
                {
                    Exception = new SecsGemException("SecsGemWorker Error", ex),
                });

                await Disconnect();
            }
        }

        internal async Task<bool> SetCommunicationState(CommunicationStateModel state)
        {
            if (state == Device.CommunicationState) return true;
            var arg = await Emit(new SecsGemCommunicationStateChangeEvent
            {
                OldState = Device.CommunicationState,
                NewState = state,
            });
            var res = arg.Return;
            Device.CommunicationState = state;
            return res;
        }

        internal async Task<bool> SetControlState(ControlStateModel state)
        {
            if (state == Device.ControlState) return true;
            var arg = await Emit(new SecsGemControlStateChangeEvent
            {
                OldState = Device.ControlState,
                NewState = state,
            });
            Device.ControlState = arg.NewState;
            return arg.Return;
        }

        public async Task<bool> TriggerAlarm(Alarm alarm)
        {
            return await _lock.ExecuteAsync(async () =>
            {
                if (!alarm.Enabled || !Device.IsControlOnline) return false;

                try
                {
                    await _tcp.SendAndWaitForReplyAsync(
                        HsmsMessage.Builder
                            .Stream(5)
                            .Func(1)
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
                    await Emit(new SecsGemErrorEvent
                    {
                        Message = "TriggerAlarm Error",
                        Exception = ex,
                    });
                }
                catch (SecsGemConnectionException)
                {
                }

                return false;
            });
        }

        public async Task<bool> SendHostDisplay(byte id, string text)
        {
            return await _lock.ExecuteAsync(async () =>
            {
                if (!Device.IsControlOnline) return false;

                try
                {
                    var reply = await _tcp.SendAndWaitForReplyAsync(
                       HsmsMessage.Builder
                           .Stream(10)
                           .Func(1)
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
                    await Emit(new SecsGemErrorEvent
                    {
                        Message = "SendHostDisplay Error",
                        Exception = ex,
                    });
                }
                catch (SecsGemConnectionException)
                {
                }

                return false;
            });
        }

        public async Task<bool> SendCollectionEvent(CollectionEvent ce)
        {
            return await _lock.ExecuteAsync(async () =>
            {
                if (!Device.IsControlOnline) return false;

                await Emit(new SecsGemGetDataVariableEvent
                {
                    Params = ce.CollectionReports.SelectMany(x => x.DataVariables)
                });

                try
                {
                    await _tcp.SendAndWaitForReplyAsync(
                        HsmsMessage.Builder
                            .Stream(6)
                            .Func(11)
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
                    await Emit(new SecsGemErrorEvent
                    {
                        Message = "SendCollectionEvent Error",
                        Exception = ex,
                    });
                }
                catch (SecsGemConnectionException)
                {
                }

                return false;
            });
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Emit(new SecsGemInitEvent());
            await _tcp.StartAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Emit(new SecsGemStopEvent());
            _tcp.Dispose();
        }
    }
}