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
                if (e is SecsGemErrorEvent ex)
                {
                    _option.DebugLog(ex.ToString());
                }

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
            await SetCommunicationState(CommunicationStateModel.CommunicationDisconnected, false);
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
            await SetCommunicationState(CommunicationStateModel.CommunicationOffline, true);

            try
            {
                while (!_cts.IsCancellationRequested && con.TcpClient.Connected)
                {
                    if (!Device.IsSelected)
                    {
                        try
                        {
                            if (_option.T7 < 100) _option.T7 = 100;
                            await TaskHelper.WaitFor(() => Device.IsSelected, 10, _option.T7 / 10, _cts.Token);
                        }
                        catch (TimeoutException)
                        {
                            await Emit(new SecsGemErrorEvent
                            {
                                Message = "T7 Timeout"
                            });
                            await Disconnect();
                            return;
                        }
                    }
                    else if (Device.CommunicationState == CommunicationStateModel.CommunicationOffline)
                    {
                        if (_option.ActiveConnect)
                        {
                            var success = await Function.CommunicationEstablish(_cts.Token);
                            if (success) continue;
                        }
                        await Task.Delay(3000, _cts.Token);
                    }
                    else
                    {
                        var reply = await _tcp.SendAndWaitForReplyAsync(
                            HsmsMessage.Builder
                                .Type(HsmsMessageType.LinkTestReq)
                                .Build(),
                            _cts.Token
                        );
                        await Task.Delay(_option.TLinkTest, _cts.Token);
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

        internal async Task<bool> SetCommunicationState(CommunicationStateModel state, bool force = false)
        {
            if (state == Device.CommunicationState) return true;
            var evt = await Emit(new SecsGemCommunicationStateChangeEvent
            {
                OldState = Device.CommunicationState,
                NewState = state,
            });
            if (evt.Accept || force)
                Device.CommunicationState = state;

            return evt.Accept;
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
            await Function.Separate();
            await Disconnect();
            _tcp.Dispose();
        }
    }
}