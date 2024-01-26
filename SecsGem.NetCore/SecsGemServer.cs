using SecsGem.NetCore.Connection;
using SecsGem.NetCore.Event.Common;
using SecsGem.NetCore.Event.Server;
using SecsGem.NetCore.Feature.Server;
using SecsGem.NetCore.Function;
using SecsGem.NetCore.Handler.Server;
using SecsGem.NetCore.Helper;
using SecsGem.NetCore.Hsms;
using SecsGem.NetCore.Mutex;
using TrafficCom.V3.Connection;

namespace SecsGem.NetCore
{
    public class SecsGemServer : IHostedService, IAsyncDisposable
    {
        public SecsGemServerFeature Feature { get; protected set; } = new();

        public GemServerDevice Device { get; protected set; } = new();

        public GemServerFunction Function { get; protected set; }

        public event SecsGemEventHandler OnEvent;

        private readonly CancellationTokenSource _cts = new();

        internal readonly SecsGemOption _option;

        internal readonly SecsGemTcpServer _tcp;

        private readonly SecsGemServerRequestHandler _requestHandler = new();

        private readonly AsyncExecutionLock _lock = new();

        private TcpConnection _host;

        public SecsGemServer(SecsGemOption option)
        {
            if (option.Target == null) throw new ArgumentException("Target");

            _option = option;
            _tcp = new(_option);
            _tcp.OnMessageReceived += OnTcpMessageReceived;
            _tcp.OnConnection += OnTcpConnection;
            _tcp.OnError += OnTcpError;
            Function = new(this);
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
                    _ = Task.Run(() => SecsGemServerWorker(con));
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
                if (OnEvent != null)
                {
                    _option.DebugLog($"Event emit: {e.Event}");
                    await OnEvent.Invoke(this, e);
                }
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
            Device.ControlState.ChangeControlState(ControlStateModel.ControlHostOffLine);
            Device.IsSelected = false;
            if (_host != null)
            {
                _host.Close();
                _host = null;
            }
        }

        private async Task SecsGemServerWorker(TcpConnection con)
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
                    _option.DebugLog("Wait for select timeout");
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
                                    .ReplyExpected()
                                    .Build()
                            );

                            var ack = reply.Root[0].GetBin();
                            if (ack == 0x0)
                            {
                                await SetCommunicationState(CommunicationStateModel.CommunicationOnline);
                            }
                            else
                            {
                                _option.DebugLog("Client reject communication establishment request");
                                await Emit(new SecsGemErrorEvent
                                {
                                    Exception = new SecsGemConnectionException("Remote Rejected Communication Establishment Request")
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
                    _option.DebugLog($"SecsGemWorker Error: {ex}");
                    await Emit(new SecsGemErrorEvent
                    {
                        Exception = ex,
                    });
                }

                await Disconnect();
            }
            catch (Exception ex)
            {
                _option.DebugLog($"SecsGemWorker Error: {ex}");
                await Emit(new SecsGemErrorEvent
                {
                    Exception = new SecsGemException("SecsGemWorker Error", ex),
                });

                await Disconnect();
            }
        }

        internal async Task SetCommunicationState(CommunicationStateModel state)
        {
            if (state == Device.CommunicationState) return;
            await Emit(new SecsGemCommunicationStateChangeEvent
            {
                OldState = Device.CommunicationState,
                NewState = state,
            });
            Device.CommunicationState = state;
        }

        public async Task StartAsync()
        {
            await StartAsync(CancellationToken.None);
        }

        public async Task StartAsync(CancellationToken ct)
        {
            await Emit(new SecsGemInitEvent());
            await _tcp.StartAsync(ct);
        }

        public async Task StopAsync()
        {
            await StopAsync(CancellationToken.None);
        }

        public async Task StopAsync(CancellationToken ct)
        {
            await DisposeAsync();
        }

        public async ValueTask DisposeAsync()
        {
            await Emit(new SecsGemStopEvent());
            await Function.Teardown();
            await Disconnect();
            _tcp.Dispose();
        }
    }
}